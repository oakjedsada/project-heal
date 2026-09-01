using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;
using MindCheck.Infrastructure.Seed;

namespace MindCheck.Infrastructure.Configurations;

public sealed class FlowTransitionConfiguration : IEntityTypeConfiguration<FlowTransition>
{
    public void Configure(EntityTypeBuilder<FlowTransition> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.FromInstrumentId).HasConversion(IdValueConverters.NullableInstrumentId);
        builder.Property(t => t.ToInstrumentId).HasConversion(IdValueConverters.InstrumentId);
        builder.Property(t => t.QuestionId).HasConversion(IdValueConverters.NullableQuestionId);
        builder.Property(t => t.ConditionType).HasConversion<string>().HasMaxLength(30);
        builder.Property(t => t.ConditionValue).HasMaxLength(200);

        builder.HasData(SeedData.FlowTransitions);
    }
}
