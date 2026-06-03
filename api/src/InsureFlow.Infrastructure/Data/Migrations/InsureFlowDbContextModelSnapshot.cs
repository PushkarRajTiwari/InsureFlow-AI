using InsureFlow.Domain.Entities;
using InsureFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace InsureFlow.Infrastructure.Data.Migrations;

[DbContext(typeof(InsureFlowDbContext))]
public partial class InsureFlowDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

        modelBuilder.Entity<User>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("GoogleId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            b.Property<string>("Name").IsRequired().HasMaxLength(256).HasColumnType("character varying(256)");
            b.HasKey("Id");
            b.HasIndex("Email");
            b.HasIndex("GoogleId").IsUnique();
            b.ToTable("Users");
        });

        modelBuilder.Entity<ConnectedMailbox>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<string>("AccessToken").IsRequired().HasColumnType("text");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("EmailAddress").IsRequired().HasMaxLength(256).HasColumnType("character varying(256)");
            b.Property<string>("RefreshToken").IsRequired().HasColumnType("text");
            b.Property<Guid>("UserId").HasColumnType("uuid");
            b.HasKey("Id");
            b.HasIndex("UserId").IsUnique();
            b.ToTable("ConnectedMailboxes");
        });

        modelBuilder.Entity<EmailMessage>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<string>("BodyPreview").IsRequired().HasMaxLength(4000).HasColumnType("character varying(4000)");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("ExternalMessageId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            b.Property<Guid>("MailboxId").HasColumnType("uuid");
            b.Property<DateTimeOffset>("ReceivedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Sender").IsRequired().HasMaxLength(512).HasColumnType("character varying(512)");
            b.Property<string>("Subject").IsRequired().HasMaxLength(512).HasColumnType("character varying(512)");
            b.Property<string>("ThreadId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            b.HasKey("Id");
            b.HasIndex("MailboxId", "ExternalMessageId").IsUnique();
            b.ToTable("EmailMessages");
        });

        modelBuilder.Entity<EmailClassification>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uuid");
            b.Property<DateTimeOffset>("ClassifiedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Category").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)");
            b.Property<decimal>("ConfidenceScore").HasPrecision(5, 4).HasColumnType("numeric(5,4)");
            b.Property<Guid>("EmailMessageId").HasColumnType("uuid");
            b.Property<string>("Reasoning").IsRequired().HasMaxLength(2000).HasColumnType("character varying(2000)");
            b.HasKey("Id");
            b.HasIndex("EmailMessageId").IsUnique();
            b.ToTable("EmailClassifications");
        });

        modelBuilder.Entity<ConnectedMailbox>(b =>
        {
            b.HasOne("InsureFlow.Domain.Entities.User", "User")
                .WithOne("ConnectedMailbox")
                .HasForeignKey<ConnectedMailbox>("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("User");
        });

        modelBuilder.Entity<EmailMessage>(b =>
        {
            b.HasOne("InsureFlow.Domain.Entities.ConnectedMailbox", "Mailbox")
                .WithMany("EmailMessages")
                .HasForeignKey("MailboxId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Mailbox");
        });

        modelBuilder.Entity<EmailClassification>(b =>
        {
            b.HasOne("InsureFlow.Domain.Entities.EmailMessage", "EmailMessage")
                .WithOne("Classification")
                .HasForeignKey<EmailClassification>("EmailMessageId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("EmailMessage");
        });

        modelBuilder.Entity<ConnectedMailbox>(b => b.Navigation("EmailMessages"));
        modelBuilder.Entity<EmailMessage>(b => b.Navigation("Classification"));
        modelBuilder.Entity<User>(b => b.Navigation("ConnectedMailbox"));
    }
}
