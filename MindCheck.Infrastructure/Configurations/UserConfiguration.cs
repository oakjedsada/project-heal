using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;

namespace MindCheck.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasConversion(IdValueConverters.UserId).ValueGeneratedNever();
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.PasswordResetToken).HasMaxLength(128);
        builder.Property(u => u.PasswordResetTokenExpiresAt);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.TokenVersion).IsRequired().HasDefaultValue(0);
        builder.Property(u => u.FailedLoginAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(u => u.LockedOutUntil);

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        // Not unique: every signed-out user starts with a null token, and
        // Postgres treats each NULL as distinct for a unique index anyway —
        // this index exists purely to make the reset-password lookup fast.
        builder.HasIndex(u => u.PasswordResetToken);
    }
}
