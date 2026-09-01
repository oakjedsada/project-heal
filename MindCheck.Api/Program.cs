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
using MindCheck.Domain.Evaluation;
using MindCheck.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT from POST /api/admin/auth/login. Enter as: Bearer {token}",
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

builder.Services.AddScoped<StartSessionUseCase>();
builder.Services.AddScoped<GetNextQuestionUseCase>();
builder.Services.AddScoped<SubmitAnswerUseCase>();
builder.Services.AddScoped<GetResultUseCase>();

builder.Services.AddScoped<CreateInstrumentUseCase>();
builder.Services.AddScoped<ListInstrumentsUseCase>();
builder.Services.AddScoped<GetInstrumentDetailUseCase>();
builder.Services.AddScoped<CreateFlowTransitionUseCase>();
builder.Services.AddScoped<ListFlowTransitionsUseCase>();
builder.Services.AddScoped<DeleteFlowTransitionUseCase>();

builder.Services.Configure<AdminAuthOptions>(builder.Configuration.GetSection(AdminAuthOptions.SectionName));

// Bound lazily via IOptions (resolved the first time the JWT handler actually
// needs it, i.e. per-request, after the host — and any test config overrides
// — are fully built). Reading AdminAuthOptions eagerly here, before Build(),
// would freeze in whatever value existed at registration time; see ADR 0006
// for the same mistake with the DB connection string.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<AdminAuthOptions>>((jwtOptions, adminAuthOptionsAccessor) =>
    {
        var adminAuthOptions = adminAuthOptionsAccessor.Value;
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = adminAuthOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = adminAuthOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(adminAuthOptions.JwtSigningKey)),
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
