using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateFolder_AspNetUsers_CreatedBy",
                table: "TemplateFolder");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateFolder_TemplateFolder_ParentId",
                table: "TemplateFolder");

            migrationBuilder.DropForeignKey(
                name: "FK_TestSuiteTemplates_TemplateFolder_FolderId",
                table: "TestSuiteTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TemplateFolder",
                table: "TemplateFolder");

            migrationBuilder.RenameTable(
                name: "TemplateFolder",
                newName: "TemplateFolders");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateFolder_ParentId",
                table: "TemplateFolders",
                newName: "IX_TemplateFolders_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateFolder_CreatedBy",
                table: "TemplateFolders",
                newName: "IX_TemplateFolders_CreatedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TemplateFolders",
                table: "TemplateFolders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateFolders_AspNetUsers_CreatedBy",
                table: "TemplateFolders",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateFolders_TemplateFolders_ParentId",
                table: "TemplateFolders",
                column: "ParentId",
                principalTable: "TemplateFolders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSuiteTemplates_TemplateFolders_FolderId",
                table: "TestSuiteTemplates",
                column: "FolderId",
                principalTable: "TemplateFolders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateFolders_AspNetUsers_CreatedBy",
                table: "TemplateFolders");

            migrationBuilder.DropForeignKey(
                name: "FK_TemplateFolders_TemplateFolders_ParentId",
                table: "TemplateFolders");

            migrationBuilder.DropForeignKey(
                name: "FK_TestSuiteTemplates_TemplateFolders_FolderId",
                table: "TestSuiteTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TemplateFolders",
                table: "TemplateFolders");

            migrationBuilder.RenameTable(
                name: "TemplateFolders",
                newName: "TemplateFolder");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateFolders_ParentId",
                table: "TemplateFolder",
                newName: "IX_TemplateFolder_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_TemplateFolders_CreatedBy",
                table: "TemplateFolder",
                newName: "IX_TemplateFolder_CreatedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TemplateFolder",
                table: "TemplateFolder",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateFolder_AspNetUsers_CreatedBy",
                table: "TemplateFolder",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateFolder_TemplateFolder_ParentId",
                table: "TemplateFolder",
                column: "ParentId",
                principalTable: "TemplateFolder",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSuiteTemplates_TemplateFolder_FolderId",
                table: "TestSuiteTemplates",
                column: "FolderId",
                principalTable: "TemplateFolder",
                principalColumn: "Id");
        }
    }
}
