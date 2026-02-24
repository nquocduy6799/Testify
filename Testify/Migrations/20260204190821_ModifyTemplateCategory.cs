using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class ModifyTemplateCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryId",
                table: "TemplateCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCategories_ParentCategoryId",
                table: "TemplateCategories",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TemplateCategories_TemplateCategories_ParentCategoryId",
                table: "TemplateCategories",
                column: "ParentCategoryId",
                principalTable: "TemplateCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TemplateCategories_TemplateCategories_ParentCategoryId",
                table: "TemplateCategories");

            migrationBuilder.DropIndex(
                name: "IX_TemplateCategories_ParentCategoryId",
                table: "TemplateCategories");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "TemplateCategories");
        }
    }
}
