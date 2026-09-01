using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;
using MindCheck.Infrastructure.Seed;

namespace MindCheck.Infrastructure.Configurations;

public sealed class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasConversion(IdValueConverters.InstrumentId).ValueGeneratedOnAdd();
        builder.Property(i => i.Code).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Version).IsRequired().HasMaxLength(20);
        builder.Property(i => i.Source).IsRequired().HasMaxLength(200);
        builder.Property(i => i.IsActive);

        builder.HasIndex(i => i.Code).IsUnique();

        builder.HasMany(i => i.Questions)
            .WithOne()
            .HasForeignKey(q => q.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.Questions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(SeedData.Instruments);
    }
}
