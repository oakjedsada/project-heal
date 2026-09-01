using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;

namespace MindCheck.Infrastructure.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasConversion(IdValueConverters.SessionId).ValueGeneratedNever();
        builder.Property(s => s.AnonToken).IsRequired().HasMaxLength(64);
        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.ConsentAt);
        builder.Property(s => s.State)
            .HasConversion(new SessionStateValueConverter())
            .HasColumnName("current_state")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(s => s.AnonToken).IsUnique();
    }
}
