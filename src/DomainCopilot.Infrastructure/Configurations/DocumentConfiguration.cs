using DomainCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DomainCopilot.Infrastructure.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Source)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Format)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Version)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ContentHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.ContentHash);
    }
}