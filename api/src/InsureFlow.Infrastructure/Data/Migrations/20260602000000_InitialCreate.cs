using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsureFlow.Infrastructure.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                GoogleId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ConnectedMailboxes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                AccessToken = table.Column<string>(type: "text", nullable: false),
                RefreshToken = table.Column<string>(type: "text", nullable: false),
                EmailAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConnectedMailboxes", x => x.Id);
                table.ForeignKey("FK_ConnectedMailboxes_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EmailMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MailboxId = table.Column<Guid>(type: "uuid", nullable: false),
                ExternalMessageId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ThreadId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Sender = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                Subject = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                BodyPreview = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailMessages", x => x.Id);
                table.ForeignKey("FK_EmailMessages_ConnectedMailboxes_MailboxId", x => x.MailboxId, "ConnectedMailboxes", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "EmailClassifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EmailMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ConfidenceScore = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                Reasoning = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                ClassifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailClassifications", x => x.Id);
                table.ForeignKey("FK_EmailClassifications_EmailMessages_EmailMessageId", x => x.EmailMessageId, "EmailMessages", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email");
        migrationBuilder.CreateIndex("IX_Users_GoogleId", "Users", "GoogleId", unique: true);
        migrationBuilder.CreateIndex("IX_ConnectedMailboxes_UserId", "ConnectedMailboxes", "UserId", unique: true);
        migrationBuilder.CreateIndex("IX_EmailMessages_MailboxId_ExternalMessageId", "EmailMessages", new[] { "MailboxId", "ExternalMessageId" }, unique: true);
        migrationBuilder.CreateIndex("IX_EmailClassifications_EmailMessageId", "EmailClassifications", "EmailMessageId", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("EmailClassifications");
        migrationBuilder.DropTable("EmailMessages");
        migrationBuilder.DropTable("ConnectedMailboxes");
        migrationBuilder.DropTable("Users");
    }
}
