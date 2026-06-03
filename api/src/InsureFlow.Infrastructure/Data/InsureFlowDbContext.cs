using InsureFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsureFlow.Infrastructure.Data;

public sealed class InsureFlowDbContext(DbContextOptions<InsureFlowDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<ConnectedMailbox> ConnectedMailboxes => Set<ConnectedMailbox>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();
    public DbSet<EmailClassification> EmailClassifications => Set<EmailClassification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsureFlowDbContext).Assembly);
    }
}
