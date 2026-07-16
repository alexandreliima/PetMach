using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase5Chat : Migration
{
    private static readonly string[] ConversationSentColumns = ["ConversationId", "SentAtUtc"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "chat");

        migrationBuilder.CreateTable(
            name: "Conversations",
            schema: "chat",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Conversations", x => x.Id);
                table.ForeignKey(
                    name: "FK_Conversations_DogMatches_MatchId",
                    column: x => x.MatchId,
                    principalSchema: "matches",
                    principalTable: "DogMatches",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Messages",
            schema: "chat",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                SenderUserId = table.Column<Guid>(type: "uuid", nullable: false),
                Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                SentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Messages", x => x.Id);
                table.ForeignKey(
                    name: "FK_Messages_AspNetUsers_SenderUserId",
                    column: x => x.SenderUserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Messages_Conversations_ConversationId",
                    column: x => x.ConversationId,
                    principalSchema: "chat",
                    principalTable: "Conversations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("""
            INSERT INTO chat."Conversations" ("Id", "MatchId", "CreatedAtUtc")
            SELECT gen_random_uuid(), match."Id", match."CreatedAtUtc"
            FROM matches."DogMatches" AS match
            WHERE NOT EXISTS (
                SELECT 1 FROM chat."Conversations" AS conversation WHERE conversation."MatchId" = match."Id");
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Conversations_MatchId",
            schema: "chat",
            table: "Conversations",
            column: "MatchId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Messages_ConversationId_SentAtUtc",
            schema: "chat",
            table: "Messages",
            columns: ConversationSentColumns);

        migrationBuilder.CreateIndex(
            name: "IX_Messages_SenderUserId",
            schema: "chat",
            table: "Messages",
            column: "SenderUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Messages",
            schema: "chat");

        migrationBuilder.DropTable(
            name: "Conversations",
            schema: "chat");
    }
}
