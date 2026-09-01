using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MindCheck.Application.Abstractions;
using MindCheck.Infrastructure.Repositories;

namespace MindCheck.Infrastructure;

public static class ServiceCollectionExtensions
{
    // Takes the live IConfiguration (not a pre-resolved string) and reads the
    // connection string inside the AddDbContext callback, which only runs when
    // a MindCheckDbContext is first resolved — i.e. after the host is fully
    // built. Resolving it eagerly here would freeze in whatever value existed
    // at registration time, silently ignoring config added afterwards (e.g.
    // WebApplicationFactory's test overrides in integration tests).
    public static IServiceCollection AddMindCheckInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MindCheckDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("MindCheck")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:MindCheck configuration.");
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IInstrumentRepository, InstrumentRepository>();
        services.AddScoped<IFlowTransitionRepository, FlowTransitionRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IResponseRepository, ResponseRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();

        return services;
    }
}
