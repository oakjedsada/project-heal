using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;
using MindCheck.Infrastructure.Seed;

namespace MindCheck.Infrastructure.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasConversion(IdValueConverters.QuestionId).ValueGeneratedOnAdd();
        builder.Property(q => q.InstrumentId).HasConversion(IdValueConverters.InstrumentId);
        builder.Property(q => q.Text).IsRequired();
        builder.Property(q => q.OrderNo);
        builder.Property(q => q.QuestionType).HasConversion<string>().HasMaxLength(30);

        builder.HasMany(q => q.Choices)
            .WithOne()
            .HasForeignKey(c => c.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(q => q.Choices).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(SeedData.Questions);
    }
}
