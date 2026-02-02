using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplateFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateFolders_AspNetUsers_CreatedBy",
                table: "TemplateFolders");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "TemplateFolders",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateFolders_CreatedBy",
                table: "TemplateFolders",
                newName: "IX_TemplateFolders_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateFolders_AspNetUsers_UserId",
                table: "TemplateFolders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateFolders_AspNetUsers_UserId",
                table: "TemplateFolders");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "TemplateFolders",
                newName: "CreatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateFolders_UserId",
                table: "TemplateFolders",
                newName: "IX_TemplateFolders_CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateFolders_AspNetUsers_CreatedBy",
                table: "TemplateFolders",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
