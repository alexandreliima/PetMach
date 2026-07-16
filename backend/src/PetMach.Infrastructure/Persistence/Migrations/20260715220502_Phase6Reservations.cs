using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase6Reservations : Migration
{
    private static readonly string[] RequesterCreatedColumns = ["RequesterUserId", "CreatedAtUtc"];
    private static readonly string[] ReservationOccurredColumns = ["ReservationId", "OccurredAtUtc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "reservations");
        migrationBuilder.CreateTable(
            name: "Reservations", schema: "reservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                RequesterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                DogId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                PaymentStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                CancelledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reservations", x => x.Id);
                table.ForeignKey(
                    name: "FK_Reservations_AspNetUsers_RequesterUserId",
                    column: x => x.RequesterUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Reservations_Dogs_DogId",
                    column: x => x.DogId,
                    principalSchema: "dogs",
                    principalTable: "Dogs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Reservations_SpaceAvailabilities_AvailabilityId",
                    column: x => x.AvailabilityId,
                    principalSchema: "partners",
                    principalTable: "SpaceAvailabilities",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(name: "IX_Reservations_AvailabilityId", schema: "reservations", table: "Reservations", column: "AvailabilityId", unique: true, filter: "\"Status\" IN ('Pending', 'Confirmed')");
        migrationBuilder.CreateIndex(name: "IX_Reservations_DogId", schema: "reservations", table: "Reservations", column: "DogId");
        migrationBuilder.CreateIndex(name: "IX_Reservations_RequesterUserId_CreatedAtUtc", schema: "reservations", table: "Reservations", columns: RequesterCreatedColumns);

        migrationBuilder.CreateTable(
            name: "ReservationHistory", schema: "reservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                FromStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                ToStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                Action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReservationHistory", x => x.Id);
                table.ForeignKey(
                    name: "FK_ReservationHistory_AspNetUsers_ActorUserId",
                    column: x => x.ActorUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ReservationHistory_Reservations_ReservationId",
                    column: x => x.ReservationId,
                    principalSchema: "reservations",
                    principalTable: "Reservations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex(name: "IX_ReservationHistory_ActorUserId", schema: "reservations", table: "ReservationHistory", column: "ActorUserId");
        migrationBuilder.CreateIndex(name: "IX_ReservationHistory_ReservationId_OccurredAtUtc", schema: "reservations", table: "ReservationHistory", columns: ReservationOccurredColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ReservationHistory", schema: "reservations");
        migrationBuilder.DropTable(name: "Reservations", schema: "reservations");
    }
}
