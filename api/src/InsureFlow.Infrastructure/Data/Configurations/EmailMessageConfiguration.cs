using InsureFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsureFlow.Infrastructure.Data.Configurations;

public sealed class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalMessageId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ThreadId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Sender).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(512).IsRequired();
        builder.Property(x => x.BodyPreview).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ReceivedAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => new { x.MailboxId, x.ExternalMessageId }).IsUnique();
        builder.HasOne(x => x.Mailbox).WithMany(x => x.EmailMessages).HasForeignKey(x => x.MailboxId).OnDelete(DeleteBehavior.Cascade);
    }
}
