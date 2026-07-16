using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase4MatchNotifications : Migration
{
    private static readonly string[] RecipientCreatedColumns = ["RecipientUserId", "CreatedAtUtc"];
    private static readonly string[] RecipientMatchColumns = ["RecipientUserId", "MatchId"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "notifications");

        migrationBuilder.CreateTable(
            name: "UserNotifications",
            schema: "notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ReadAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserNotifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserNotifications_AspNetUsers_RecipientUserId",
                    column: x => x.RecipientUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserNotifications_DogMatches_MatchId",
                    column: x => x.MatchId,
                    principalSchema: "matches",
                    principalTable: "DogMatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_MatchId",
            schema: "notifications",
            table: "UserNotifications",
            column: "MatchId");

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_RecipientUserId_CreatedAtUtc",
            schema: "notifications",
            table: "UserNotifications",
            columns: RecipientCreatedColumns);

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_RecipientUserId_MatchId",
            schema: "notifications",
            table: "UserNotifications",
            columns: RecipientMatchColumns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserNotifications",
            schema: "notifications");
    }
}
