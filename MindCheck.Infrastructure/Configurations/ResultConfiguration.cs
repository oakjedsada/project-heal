using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;

namespace MindCheck.Infrastructure.Configurations;

public sealed class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.SessionId).HasConversion(IdValueConverters.SessionId);
        builder.Property(r => r.InstrumentId).HasConversion(IdValueConverters.InstrumentId);
        builder.Property(r => r.TotalScore);
        builder.Property(r => r.Level).IsRequired().HasMaxLength(50);
        builder.Property(r => r.NextAction).IsRequired().HasMaxLength(30);

        builder.HasIndex(r => r.SessionId);
    }
}
