using InsureFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsureFlow.Infrastructure.Data.Configurations;

public sealed class ConnectedMailboxConfiguration : IEntityTypeConfiguration<ConnectedMailbox>
{
    public void Configure(EntityTypeBuilder<ConnectedMailbox> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AccessToken).IsRequired();
        builder.Property(x => x.RefreshToken).IsRequired();
        builder.Property(x => x.EmailAddress).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasOne(x => x.User).WithOne(x => x.ConnectedMailbox).HasForeignKey<ConnectedMailbox>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
