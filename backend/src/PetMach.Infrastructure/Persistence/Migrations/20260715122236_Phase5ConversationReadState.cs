using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetMach.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase5ConversationReadState : Migration
{
    private static readonly string[] ConversationUserColumns = ["ConversationId", "UserId"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ConversationReadStates",
            schema: "chat",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                LastReadMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                LastReadMessageAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConversationReadStates", x => x.Id);
                table.ForeignKey(
                    name: "FK_ConversationReadStates_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalSchema: "identity",
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ConversationReadStates_Conversations_ConversationId",
                    column: x => x.ConversationId,
                    principalSchema: "chat",
                    principalTable: "Conversations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ConversationReadStates_Messages_LastReadMessageId",
                    column: x => x.LastReadMessageId,
                    principalSchema: "chat",
                    principalTable: "Messages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ConversationReadStates_ConversationId_UserId",
            schema: "chat",
            table: "ConversationReadStates",
            columns: ConversationUserColumns,
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ConversationReadStates_LastReadMessageId",
            schema: "chat",
            table: "ConversationReadStates",
            column: "LastReadMessageId");

        migrationBuilder.CreateIndex(
            name: "IX_ConversationReadStates_UserId",
            schema: "chat",
            table: "ConversationReadStates",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ConversationReadStates",
            schema: "chat");
    }
}
