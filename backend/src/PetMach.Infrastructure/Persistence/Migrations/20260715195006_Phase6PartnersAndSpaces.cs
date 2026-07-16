using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase6PartnersAndSpaces : Migration
{
    private static readonly string[] EstablishmentActiveColumns = ["EstablishmentId", "IsActive"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "partners");

        migrationBuilder.CreateTable(
            name: "Establishments",
            schema: "partners",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                LegalName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                RegistrationNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                TimeZoneId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Establishments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Establishments_AspNetUsers_OwnerUserId",
                    column: x => x.OwnerUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Spaces",
            schema: "partners",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EstablishmentId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                Capacity = table.Column<int>(type: "integer", nullable: false),
                InformationalPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Spaces", x => x.Id);
                table.ForeignKey(
                    name: "FK_Spaces_Establishments_EstablishmentId",
                    column: x => x.EstablishmentId,
                    principalSchema: "partners",
                    principalTable: "Establishments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Establishments_OwnerUserId",
            schema: "partners",
            table: "Establishments",
            column: "OwnerUserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Establishments_RegistrationNumber",
            schema: "partners",
            table: "Establishments",
            column: "RegistrationNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Spaces_EstablishmentId_IsActive",
            schema: "partners",
            table: "Spaces",
            columns: EstablishmentActiveColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Spaces",
            schema: "partners");

        migrationBuilder.DropTable(
            name: "Establishments",
            schema: "partners");
    }
}
