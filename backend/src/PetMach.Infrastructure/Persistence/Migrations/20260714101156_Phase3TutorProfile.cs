using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase3TutorProfile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "tutors");

        migrationBuilder.CreateTable(
            name: "TutorProfiles",
            schema: "tutors",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Biography = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ShowCity = table.Column<bool>(type: "boolean", nullable: false),
                AllowDiscovery = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TutorProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_TutorProfiles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TutorProfiles_UserId",
            schema: "tutors",
            table: "TutorProfiles",
            column: "UserId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TutorProfiles",
            schema: "tutors");
    }
}
