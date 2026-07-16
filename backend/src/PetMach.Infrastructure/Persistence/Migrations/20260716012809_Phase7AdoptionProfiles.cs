using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase7AdoptionProfiles : Migration
{
    private static readonly string[] StatusCreatedColumns = ["Status", "CreatedAtUtc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "adoption");

        migrationBuilder.CreateTable(
            name: "Profiles",
            schema: "adoption",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DogId = table.Column<Guid>(type: "uuid", nullable: false),
                PublisherUserId = table.Column<Guid>(type: "uuid", nullable: false),
                Story = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Requirements = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                TermsVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                TermsAcceptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Profiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_Profiles_AspNetUsers_PublisherUserId",
                    column: x => x.PublisherUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Profiles_Dogs_DogId",
                    column: x => x.DogId,
                    principalSchema: "dogs",
                    principalTable: "Dogs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Profiles_DogId",
            schema: "adoption",
            table: "Profiles",
            column: "DogId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Profiles_PublisherUserId",
            schema: "adoption",
            table: "Profiles",
            column: "PublisherUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Profiles_Status_CreatedAtUtc",
            schema: "adoption",
            table: "Profiles",
            columns: StatusCreatedColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Profiles",
            schema: "adoption");
    }
}
