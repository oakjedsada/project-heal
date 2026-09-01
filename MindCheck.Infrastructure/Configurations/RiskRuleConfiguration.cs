using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;
using MindCheck.Infrastructure.Conversions;
using MindCheck.Infrastructure.Seed;

namespace MindCheck.Infrastructure.Configurations;

public sealed class RiskRuleConfiguration : IEntityTypeConfiguration<RiskRule>
{
    public void Configure(EntityTypeBuilder<RiskRule> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.InstrumentId).HasConversion(IdValueConverters.InstrumentId);
        builder.Property(r => r.QuestionId).HasConversion(IdValueConverters.QuestionId);
        builder.Property(r => r.Action).IsRequired().HasMaxLength(50);

        builder.OwnsOne(r => r.Condition, condition =>
        {
            condition.Property(x => x.Operator).HasColumnName("operator").HasConversion<string>().HasMaxLength(30);
            condition.Property(x => x.Threshold).HasColumnName("threshold");

            condition.HasData(SeedData.RiskRules.Select(r => new
            {
                RiskRuleId = r.Id,
                r.Condition.Operator,
                r.Condition.Threshold
            }));
        });

        builder.Navigation(r => r.Condition).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(SeedData.RiskRules.Select(r => new { r.Id, r.InstrumentId, r.QuestionId, r.Action }));
    }
}
