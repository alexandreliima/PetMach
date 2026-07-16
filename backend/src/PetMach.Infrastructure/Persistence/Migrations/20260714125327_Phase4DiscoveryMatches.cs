using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0161, CA1861 // Generated migration follows EF Core's required structure.

namespace PetMach.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase4DiscoveryMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "moderation");

            migrationBuilder.EnsureSchema(
                name: "discovery");

            migrationBuilder.EnsureSchema(
                name: "matches");

            migrationBuilder.CreateTable(
                name: "BlockedUsers",
                schema: "moderation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedUsers", x => x.Id);
                    table.CheckConstraint("CK_BlockedUsers_NoSelfBlock", "\"UserId\" <> \"BlockedUserId\"");
                    table.ForeignKey(
                        name: "FK_BlockedUsers_AspNetUsers_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BlockedUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DogLikes",
                schema: "discovery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetDogId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogLikes", x => x.Id);
                    table.CheckConstraint("CK_DogLikes_NoSelfLike", "\"SourceDogId\" <> \"TargetDogId\"");
                    table.ForeignKey(
                        name: "FK_DogLikes_Dogs_SourceDogId",
                        column: x => x.SourceDogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DogLikes_Dogs_TargetDogId",
                        column: x => x.TargetDogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DogMatches",
                schema: "matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DogAId = table.Column<Guid>(type: "uuid", nullable: false),
                    DogBId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogMatches", x => x.Id);
                    table.CheckConstraint("CK_DogMatches_DistinctDogs", "\"DogAId\" <> \"DogBId\"");
                    table.ForeignKey(
                        name: "FK_DogMatches_Dogs_DogAId",
                        column: x => x.DogAId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DogMatches_Dogs_DogBId",
                        column: x => x.DogBId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DogPasses",
                schema: "discovery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDogId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetDogId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogPasses", x => x.Id);
                    table.CheckConstraint("CK_DogPasses_NoSelfPass", "\"SourceDogId\" <> \"TargetDogId\"");
                    table.ForeignKey(
                        name: "FK_DogPasses_Dogs_SourceDogId",
                        column: x => x.SourceDogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DogPasses_Dogs_TargetDogId",
                        column: x => x.TargetDogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DogPreferences",
                schema: "discovery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DogId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaximumDistanceKm = table.Column<int>(type: "integer", nullable: true),
                    MinimumAgeYears = table.Column<int>(type: "integer", nullable: true),
                    MaximumAgeYears = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DogPreferences_Dogs_DogId",
                        column: x => x.DogId,
                        principalSchema: "dogs",
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUsers_BlockedUserId",
                schema: "moderation",
                table: "BlockedUsers",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUsers_UserId_BlockedUserId",
                schema: "moderation",
                table: "BlockedUsers",
                columns: new[] { "UserId", "BlockedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DogLikes_SourceDogId_TargetDogId",
                schema: "discovery",
                table: "DogLikes",
                columns: new[] { "SourceDogId", "TargetDogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DogLikes_TargetDogId",
                schema: "discovery",
                table: "DogLikes",
                column: "TargetDogId");

            migrationBuilder.CreateIndex(
                name: "IX_DogMatches_DogAId_DogBId",
                schema: "matches",
                table: "DogMatches",
                columns: new[] { "DogAId", "DogBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DogMatches_DogBId",
                schema: "matches",
                table: "DogMatches",
                column: "DogBId");

            migrationBuilder.CreateIndex(
                name: "IX_DogPasses_SourceDogId_TargetDogId",
                schema: "discovery",
                table: "DogPasses",
                columns: new[] { "SourceDogId", "TargetDogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DogPasses_TargetDogId",
                schema: "discovery",
                table: "DogPasses",
                column: "TargetDogId");

            migrationBuilder.CreateIndex(
                name: "IX_DogPreferences_DogId",
                schema: "discovery",
                table: "DogPreferences",
                column: "DogId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedUsers",
                schema: "moderation");

            migrationBuilder.DropTable(
                name: "DogLikes",
                schema: "discovery");

            migrationBuilder.DropTable(
                name: "DogMatches",
                schema: "matches");

            migrationBuilder.DropTable(
                name: "DogPasses",
                schema: "discovery");

            migrationBuilder.DropTable(
                name: "DogPreferences",
                schema: "discovery");
        }
    }
}
