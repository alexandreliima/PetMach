using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase6SpaceAvailability : Migration
{
    private static readonly string[] AvailabilityPeriodColumns = ["SpaceId", "StartsAtUtc", "EndsAtUtc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SpaceAvailabilities",
            schema: "partners",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SpaceId = table.Column<Guid>(type: "uuid", nullable: false),
                StartsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SpaceAvailabilities", x => x.Id);
                table.CheckConstraint("CK_SpaceAvailabilities_Chronological", "\"StartsAtUtc\" < \"EndsAtUtc\"");
                table.ForeignKey(
                    name: "FK_SpaceAvailabilities_Spaces_SpaceId",
                    column: x => x.SpaceId,
                    principalSchema: "partners",
                    principalTable: "Spaces",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SpaceAvailabilities_SpaceId_StartsAtUtc_EndsAtUtc",
            schema: "partners",
            table: "SpaceAvailabilities",
            columns: AvailabilityPeriodColumns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SpaceAvailabilities",
            schema: "partners");
    }
}
