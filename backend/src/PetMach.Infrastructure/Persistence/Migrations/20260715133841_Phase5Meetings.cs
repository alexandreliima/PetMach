using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase5Meetings : Migration
{
    private static readonly string[] MatchCreatedColumns = ["MatchId", "CreatedAtUtc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "meetings");

        migrationBuilder.CreateTable(
            name: "DogMeetings",
            schema: "meetings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                ProposedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                ScheduledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                PlaceName = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DogMeetings", x => x.Id);
                table.ForeignKey(
                    name: "FK_DogMeetings_AspNetUsers_ProposedByUserId",
                    column: x => x.ProposedByUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DogMeetings_DogMatches_MatchId",
                    column: x => x.MatchId,
                    principalSchema: "matches",
                    principalTable: "DogMatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DogMeetings_MatchId_CreatedAtUtc",
            schema: "meetings",
            table: "DogMeetings",
            columns: MatchCreatedColumns);

        migrationBuilder.CreateIndex(
            name: "IX_DogMeetings_ProposedByUserId",
            schema: "meetings",
            table: "DogMeetings",
            column: "ProposedByUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DogMeetings",
            schema: "meetings");
    }
}
