using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161, CA1861 // Generated migration follows EF Core's required structure.

namespace PetMach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase3DogsAndHealth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "health");

            migrationBuilder.EnsureSchema(
                name: "dogs");

            migrationBuilder.CreateTable(
                name: "Dogs",
                schema: "dogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ApproximateAge = table.Column<bool>(type: "boolean", nullable: false),
                    Sex = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Breed = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Size = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Neutered = table.Column<bool>(type: "boolean", nullable: false),
                    Temperament = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EnergyLevel = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SociabilityWithDogs = table.Column<int>(type: "integer", nullable: false),
                    SociabilityWithPeople = table.Column<int>(type: "integer", nullable: false),
                    SociabilityWithChildren = table.Column<int>(type: "integer", nullable: false),
                    Restrictions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SpecialNeeds = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Biography = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Goal = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dogs_AspNetUsers_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DewormingRecords",
                schema: "health",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AppliedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDoseOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DewormingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DewormingRecords_Dogs_DogId",
                        column: x => x.DogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DogPhotos",
                schema: "dogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DogId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Length = table.Column<long>(type: "bigint", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DogPhotos_Dogs_DogId",
                        column: x => x.DogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DogVaccinations",
                schema: "health",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DogId = table.Column<Guid>(type: "uuid", nullable: false),
                    VaccineName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AppliedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDoseOn = table.Column<DateOnly>(type: "date", nullable: true),
                    ProtectedProofKey = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogVaccinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DogVaccinations_Dogs_DogId",
                        column: x => x.DogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DewormingRecords_DogId_AppliedOn",
                schema: "health",
                table: "DewormingRecords",
                columns: new[] { "DogId", "AppliedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_DogPhotos_DogId_IsPrimary",
                schema: "dogs",
                table: "DogPhotos",
                columns: new[] { "DogId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_OwnerUserId_Status",
                schema: "dogs",
                table: "Dogs",
                columns: new[] { "OwnerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DogVaccinations_DogId_AppliedOn",
                schema: "health",
                table: "DogVaccinations",
                columns: new[] { "DogId", "AppliedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DewormingRecords",
                schema: "health");

            migrationBuilder.DropTable(
                name: "DogPhotos",
                schema: "dogs");

            migrationBuilder.DropTable(
                name: "DogVaccinations",
                schema: "health");

            migrationBuilder.DropTable(
                name: "Dogs",
                schema: "dogs");
        }
    }
}
