using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;
using MindCheck.Infrastructure.Seed;

namespace MindCheck.Infrastructure.Configurations;

public sealed class ChoiceConfiguration : IEntityTypeConfiguration<Choice>
{
    public void Configure(EntityTypeBuilder<Choice> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(IdValueConverters.ChoiceId).ValueGeneratedOnAdd();
        builder.Property(c => c.QuestionId).HasConversion(IdValueConverters.QuestionId);
        builder.Property(c => c.Label).IsRequired().HasMaxLength(500);
        builder.Property(c => c.Score);
        builder.Property(c => c.OrderNo);

        builder.HasData(SeedData.Choices);
    }
}
