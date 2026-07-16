using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase7AdoptionApplications : Migration
{
    private static readonly string[] ApplicationOccurredColumns = ["ApplicationId", "OccurredAtUtc"];
    private static readonly string[] ApplicantCreatedColumns = ["ApplicantUserId", "CreatedAtUtc"];
    private static readonly string[] ProfileApplicantColumns = ["ProfileId", "ApplicantUserId"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Applications",
            schema: "adoption",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                Motivation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                Experience = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                HousingContext = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                TermsVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                TermsAcceptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Applications", x => x.Id);
                table.ForeignKey(
                    name: "FK_Applications_AspNetUsers_ApplicantUserId",
                    column: x => x.ApplicantUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Applications_Profiles_ProfileId",
                    column: x => x.ProfileId,
                    principalSchema: "adoption",
                    principalTable: "Profiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ApplicationHistory",
            schema: "adoption",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                FromStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                ToStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationHistory", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApplicationHistory_Applications_ApplicationId",
                    column: x => x.ApplicationId,
                    principalSchema: "adoption",
                    principalTable: "Applications",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ApplicationHistory_AspNetUsers_ActorUserId",
                    column: x => x.ActorUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationHistory_ActorUserId",
            schema: "adoption",
            table: "ApplicationHistory",
            column: "ActorUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationHistory_ApplicationId_OccurredAtUtc",
            schema: "adoption",
            table: "ApplicationHistory",
            columns: ApplicationOccurredColumns);

        migrationBuilder.CreateIndex(
            name: "IX_Applications_ApplicantUserId_CreatedAtUtc",
            schema: "adoption",
            table: "Applications",
            columns: ApplicantCreatedColumns);

        migrationBuilder.CreateIndex(
            name: "IX_Applications_ProfileId",
            schema: "adoption",
            table: "Applications",
            column: "ProfileId",
            unique: true,
            filter: "\"Status\" = 'Approved'");

        migrationBuilder.CreateIndex(
            name: "IX_Applications_ProfileId_ApplicantUserId",
            schema: "adoption",
            table: "Applications",
            columns: ProfileApplicantColumns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ApplicationHistory",
            schema: "adoption");

        migrationBuilder.DropTable(
            name: "Applications",
            schema: "adoption");
    }
}
