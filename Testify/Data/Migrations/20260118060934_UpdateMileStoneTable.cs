using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMileStoneTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Projects");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Milestones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Milestones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Milestones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Milestones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Milestones",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Milestones");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Milestones");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
