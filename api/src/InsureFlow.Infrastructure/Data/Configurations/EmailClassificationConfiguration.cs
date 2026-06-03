using InsureFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsureFlow.Infrastructure.Data.Configurations;

public sealed class EmailClassificationConfiguration : IEntityTypeConfiguration<EmailClassification>
{
    public void Configure(EntityTypeBuilder<EmailClassification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(x => x.ConfidenceScore).HasPrecision(5, 4).IsRequired();
        builder.Property(x => x.Reasoning).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ClassifiedAt).IsRequired();
        builder.HasIndex(x => x.EmailMessageId).IsUnique();
        builder.HasOne(x => x.EmailMessage).WithOne(x => x.Classification).HasForeignKey<EmailClassification>(x => x.EmailMessageId).OnDelete(DeleteBehavior.Cascade);
    }
}
