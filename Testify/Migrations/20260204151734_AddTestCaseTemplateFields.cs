using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class AddTestCaseTemplateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TestCaseTemplates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TestCaseTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TestCaseTemplates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TestCaseTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TestCaseTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Postconditions",
                table: "TestCaseTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Preconditions",
                table: "TestCaseTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TestCaseTemplates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TestCaseTemplates",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "Postconditions",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "Preconditions",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TestCaseTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TestCaseTemplates");
        }
    }
}
