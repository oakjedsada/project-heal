using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;
using MindCheck.Infrastructure.Seed;

namespace MindCheck.Infrastructure.Configurations;

public sealed class ScoringRuleConfiguration : IEntityTypeConfiguration<ScoringRule>
{
    public void Configure(EntityTypeBuilder<ScoringRule> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.InstrumentId).HasConversion(IdValueConverters.InstrumentId);
        builder.Property(r => r.Level).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Interpretation).IsRequired();
        builder.Property(r => r.Advice).IsRequired();

        builder.OwnsOne(r => r.Range, range =>
        {
            range.Property(x => x.Min).HasColumnName("min_score");
            range.Property(x => x.Max).HasColumnName("max_score");

            range.HasData(SeedData.ScoringRules.Select(r => new
            {
                ScoringRuleId = r.Id,
                r.Range.Min,
                r.Range.Max
            }));
        });

        builder.Navigation(r => r.Range).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(SeedData.ScoringRules.Select(r => new { r.Id, r.InstrumentId, r.Level, r.Interpretation, r.Advice }));
    }
}
