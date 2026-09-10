using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MindCheck.Api.Auth;
using MindCheck.Api.ErrorHandling;
using MindCheck.Api.HelpResources;
using MindCheck.Application.Abstractions;
using MindCheck.Application.UseCases;
using MindCheck.Application.UseCases.Admin;
using MindCheck.Domain.Entities;
using MindCheck.Domain.Evaluation;
using MindCheck.Domain.ValueObjects;
using MindCheck.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT from POST /api/auth/login. Enter as: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddMindCheckInfrastructure(builder.Configuration);

// Only needed once client and api are on different origins (a real deploy,
// e.g. two separate Railway services). Local dev never hits this: Vite's
// dev-server proxy makes /api same-origin, so no browser CORS check applies.
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        if (corsAllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsAllowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IScoringEvaluator, ScoringEvaluator>();
builder.Services.AddScoped<IRiskEvaluator, RiskEvaluator>();
builder.Services.AddScoped<IAssessmentFlowEngine, AssessmentFlowEngine>();
builder.Services.AddScoped<IHelpResourceProvider, ConfigurationHelpResourceProvider>();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
builder.Services.AddScoped<IAuthTokenGenerator, JwtAuthTokenGenerator>();

builder.Services.AddScoped<StartSessionUseCase>();
builder.Services.AddScoped<GetNextQuestionUseCase>();
builder.Services.AddScoped<SubmitAnswerUseCase>();
builder.Services.AddScoped<GetResultUseCase>();
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<LoginUseCase>();

builder.Services.AddScoped<CreateInstrumentUseCase>();
builder.Services.AddScoped<ListInstrumentsUseCase>();
builder.Services.AddScoped<GetInstrumentDetailUseCase>();
builder.Services.AddScoped<CreateFlowTransitionUseCase>();
builder.Services.AddScoped<ListFlowTransitionsUseCase>();
builder.Services.AddScoped<DeleteFlowTransitionUseCase>();
builder.Services.AddScoped<GetDashboardStatsUseCase>();
builder.Services.AddScoped<ListUsersUseCase>();
builder.Services.AddScoped<CreateUserUseCase>();
builder.Services.AddScoped<ChangeUserRoleUseCase>();
builder.Services.AddScoped<DeleteUserUseCase>();

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));

// Bound lazily via IOptions (resolved the first time the JWT handler actually
// needs it, i.e. per-request, after the host — and any test config overrides
// — are fully built). Reading AuthOptions eagerly here, before Build(),
// would freeze in whatever value existed at registration time; see ADR 0006
// for the same mistake with the DB connection string.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<AuthOptions>>((jwtOptions, authOptionsAccessor) =>
    {
        var authOptions = authOptionsAccessor.Value;
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSigningKey)),
            ValidateLifetime = true,
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MindCheckDbContext>();
    db.Database.Migrate();

    // First-boot bootstrap: the shared-password admin login is gone, so the
    // only way an Admin account ever comes to exist is this one-time seed
    // when the Users table is empty. Afterward, admins are managed entirely
    // through AdminUsersController — these config values are never read again.
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    if (!await userRepository.AnyAsync(CancellationToken.None))
    {
        var authOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthOptions>>().Value;
        if (!string.IsNullOrWhiteSpace(authOptions.BootstrapAdminUsername) &&
            !string.IsNullOrWhiteSpace(authOptions.BootstrapAdminEmail) &&
            !string.IsNullOrWhiteSpace(authOptions.BootstrapAdminPassword))
        {
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var admin = new User(
                new UserId(Guid.NewGuid()),
                authOptions.BootstrapAdminUsername,
                authOptions.BootstrapAdminEmail,
                passwordHasher.Hash(authOptions.BootstrapAdminPassword),
                UserRole.Admin,
                DateTimeOffset.UtcNow);
            await userRepository.AddAsync(admin, CancellationToken.None);
        }
    }
}

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can boot this app in integration tests.
public partial class Program
{
}
