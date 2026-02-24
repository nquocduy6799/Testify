using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class AddChatFKAndUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatPinnedMessages_AspNetUsers_PinnedById",
                table: "ChatPinnedMessages");

            migrationBuilder.DropTable(
                name: "ChatNotifications");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomParticipants_RoomId",
                table: "ChatRoomParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ChatPinnedMessages_PinnedById",
                table: "ChatPinnedMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessageReads_MessageId",
                table: "ChatMessageReads");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessageReactions_MessageId",
                table: "ChatMessageReactions");

            migrationBuilder.DropColumn(
                name: "PinnedById",
                table: "ChatPinnedMessages");

            migrationBuilder.AlterColumn<string>(
                name: "PinnedByUserId",
                table: "ChatPinnedMessages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Reaction",
                table: "ChatMessageReactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_RoomId_UserId",
                table: "ChatRoomParticipants",
                columns: new[] { "RoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatPinnedMessages_PinnedByUserId",
                table: "ChatPinnedMessages",
                column: "PinnedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReads_MessageId_UserId",
                table: "ChatMessageReads",
                columns: new[] { "MessageId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReactions_MessageId_UserId_Reaction",
                table: "ChatMessageReactions",
                columns: new[] { "MessageId", "UserId", "Reaction" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatPinnedMessages_AspNetUsers_PinnedByUserId",
                table: "ChatPinnedMessages",
                column: "PinnedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatPinnedMessages_AspNetUsers_PinnedByUserId",
                table: "ChatPinnedMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoomParticipants_RoomId_UserId",
                table: "ChatRoomParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ChatPinnedMessages_PinnedByUserId",
                table: "ChatPinnedMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessageReads_MessageId_UserId",
                table: "ChatMessageReads");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessageReactions_MessageId_UserId_Reaction",
                table: "ChatMessageReactions");

            migrationBuilder.AlterColumn<string>(
                name: "PinnedByUserId",
                table: "ChatPinnedMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "PinnedById",
                table: "ChatPinnedMessages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reaction",
                table: "ChatMessageReactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "ChatNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    NotificationType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatNotifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatNotifications_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatNotifications_ChatRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "ChatRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomParticipants_RoomId",
                table: "ChatRoomParticipants",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatPinnedMessages_PinnedById",
                table: "ChatPinnedMessages",
                column: "PinnedById");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReads_MessageId",
                table: "ChatMessageReads",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReactions_MessageId",
                table: "ChatMessageReactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatNotifications_MessageId",
                table: "ChatNotifications",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatNotifications_RoomId",
                table: "ChatNotifications",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatNotifications_UserId",
                table: "ChatNotifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatPinnedMessages_AspNetUsers_PinnedById",
                table: "ChatPinnedMessages",
                column: "PinnedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
