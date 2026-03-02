using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class FixCallSessionForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CallSessions_AspNetUsers_CalleeId",
                table: "CallSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CallSessions_AspNetUsers_CallerId",
                table: "CallSessions");

            migrationBuilder.DropIndex(
                name: "IX_CallSessions_CalleeId",
                table: "CallSessions");

            migrationBuilder.DropIndex(
                name: "IX_CallSessions_CallerId",
                table: "CallSessions");

            migrationBuilder.DropColumn(
                name: "CalleeId",
                table: "CallSessions");

            migrationBuilder.DropColumn(
                name: "CallerId",
                table: "CallSessions");

            migrationBuilder.AlterColumn<string>(
                name: "CallerUserId",
                table: "CallSessions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CalleeUserId",
                table: "CallSessions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_CalleeUserId",
                table: "CallSessions",
                column: "CalleeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_CallerUserId",
                table: "CallSessions",
                column: "CallerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CallSessions_AspNetUsers_CalleeUserId",
                table: "CallSessions",
                column: "CalleeUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CallSessions_AspNetUsers_CallerUserId",
                table: "CallSessions",
                column: "CallerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CallSessions_AspNetUsers_CalleeUserId",
                table: "CallSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CallSessions_AspNetUsers_CallerUserId",
                table: "CallSessions");

            migrationBuilder.DropIndex(
                name: "IX_CallSessions_CalleeUserId",
                table: "CallSessions");

            migrationBuilder.DropIndex(
                name: "IX_CallSessions_CallerUserId",
                table: "CallSessions");

            migrationBuilder.AlterColumn<string>(
                name: "CallerUserId",
                table: "CallSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CalleeUserId",
                table: "CallSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CalleeId",
                table: "CallSessions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallerId",
                table: "CallSessions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_CalleeId",
                table: "CallSessions",
                column: "CalleeId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_CallerId",
                table: "CallSessions",
                column: "CallerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CallSessions_AspNetUsers_CalleeId",
                table: "CallSessions",
                column: "CalleeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CallSessions_AspNetUsers_CallerId",
                table: "CallSessions",
                column: "CallerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
