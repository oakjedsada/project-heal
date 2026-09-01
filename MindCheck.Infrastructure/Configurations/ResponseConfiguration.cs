using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MindCheck.Domain.Entities;
using MindCheck.Infrastructure.Conversions;

namespace MindCheck.Infrastructure.Configurations;

public sealed class ResponseConfiguration : IEntityTypeConfiguration<Response>
{
    public void Configure(EntityTypeBuilder<Response> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.SessionId).HasConversion(IdValueConverters.SessionId);
        builder.Property(r => r.QuestionId).HasConversion(IdValueConverters.QuestionId);
        builder.Property(r => r.ChoiceId).HasConversion(IdValueConverters.ChoiceId);
        builder.Property(r => r.AnsweredAt).IsRequired();

        builder.HasIndex(r => new { r.SessionId, r.QuestionId });
    }
}
