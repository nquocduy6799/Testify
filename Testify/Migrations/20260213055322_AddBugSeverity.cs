using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class AddBugSeverity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BugSeverity",
                table: "KanbanTasks",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BugSeverity",
                table: "KanbanTasks");
        }
    }
}
