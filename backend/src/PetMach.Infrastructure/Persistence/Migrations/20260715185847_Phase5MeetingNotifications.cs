using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase5MeetingNotifications : Migration
{
    private static readonly string[] RecipientTypeMatchColumns = ["RecipientUserId", "Type", "MatchId"];
    private static readonly string[] RecipientTypeMeetingColumns = ["RecipientUserId", "Type", "MeetingId"];
    private static readonly string[] RecipientMatchColumns = ["RecipientUserId", "MatchId"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserNotifications_RecipientUserId_MatchId",
            schema: "notifications",
            table: "UserNotifications");

        migrationBuilder.AlterColumn<Guid>(
            name: "MatchId",
            schema: "notifications",
            table: "UserNotifications",
            type: "uuid",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uuid");

        migrationBuilder.AddColumn<Guid>(
            name: "MeetingId",
            schema: "notifications",
            table: "UserNotifications",
            type: "uuid",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Type",
            schema: "notifications",
            table: "UserNotifications",
            type: "character varying(64)",
            maxLength: 64,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql("""
            UPDATE notifications."UserNotifications"
            SET "Type" = 'match.created'
            WHERE "MatchId" IS NOT NULL AND "Type" = '';
            """);

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_MeetingId",
            schema: "notifications",
            table: "UserNotifications",
            column: "MeetingId");

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_RecipientUserId_Type_MatchId",
            schema: "notifications",
            table: "UserNotifications",
            columns: RecipientTypeMatchColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_RecipientUserId_Type_MeetingId",
            schema: "notifications",
            table: "UserNotifications",
            columns: RecipientTypeMeetingColumns,
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_UserNotifications_DogMeetings_MeetingId",
            schema: "notifications",
            table: "UserNotifications",
            column: "MeetingId",
            principalSchema: "meetings",
            principalTable: "DogMeetings",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM notifications."UserNotifications"
            WHERE "MeetingId" IS NOT NULL;
            """);

        migrationBuilder.DropForeignKey(
            name: "FK_UserNotifications_DogMeetings_MeetingId",
            schema: "notifications",
            table: "UserNotifications");

        migrationBuilder.DropIndex(
            name: "IX_UserNotifications_MeetingId",
            schema: "notifications",
            table: "UserNotifications");

        migrationBuilder.DropIndex(
            name: "IX_UserNotifications_RecipientUserId_Type_MatchId",
            schema: "notifications",
            table: "UserNotifications");

        migrationBuilder.DropIndex(
            name: "IX_UserNotifications_RecipientUserId_Type_MeetingId",
            schema: "notifications",
            table: "UserNotifications");

        migrationBuilder.DropColumn(
            name: "MeetingId",
            schema: "notifications",
            table: "UserNotifications");

        migrationBuilder.DropColumn(
            name: "Type",
            schema: "notifications",
            table: "UserNotifications");

        migrationBuilder.AlterColumn<Guid>(
            name: "MatchId",
            schema: "notifications",
            table: "UserNotifications",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserNotifications_RecipientUserId_MatchId",
            schema: "notifications",
            table: "UserNotifications",
            columns: RecipientMatchColumns,
            unique: true);
    }
}
