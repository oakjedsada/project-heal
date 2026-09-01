using Microsoft.EntityFrameworkCore;
using MindCheck.Domain.Entities;

namespace MindCheck.Infrastructure;

public sealed class MindCheckDbContext : DbContext
{
    public MindCheckDbContext(DbContextOptions<MindCheckDbContext> options)
        : base(options)
    {
    }

    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<ScoringRule> ScoringRules => Set<ScoringRule>();
    public DbSet<RiskRule> RiskRules => Set<RiskRule>();
    public DbSet<FlowTransition> FlowTransitions => Set<FlowTransition>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Response> Responses => Set<Response>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MindCheckDbContext).Assembly);
    }
}
