using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase7ModerationActions : Migration
{
    private static readonly string[] ModeratorOccurredColumns = ["ModeratorUserId", "OccurredAtUtc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Actions",
            schema: "moderation",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                ModeratorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                ActionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                TargetType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Actions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Actions_AspNetUsers_ModeratorUserId",
                    column: x => x.ModeratorUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Actions_Reports_ReportId",
                    column: x => x.ReportId,
                    principalSchema: "moderation",
                    principalTable: "Reports",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Actions_ModeratorUserId_OccurredAtUtc",
            schema: "moderation",
            table: "Actions",
            columns: ModeratorOccurredColumns);

        migrationBuilder.CreateIndex(
            name: "IX_Actions_ReportId",
            schema: "moderation",
            table: "Actions",
            column: "ReportId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Actions",
            schema: "moderation");
    }
}
