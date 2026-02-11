using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testify.Migrations
{
    /// <inheritdoc />
    public partial class ModifyTestRunTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_AspNetUsers_ExecutedById",
                table: "TestRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_TestPlans_PlanId",
                table: "TestRuns");

            migrationBuilder.DropIndex(
                name: "IX_TestRuns_ExecutedById",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "FileSizeInBytes",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "ExecutedById",
                table: "TestRuns");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "TestRunStepAttachments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "TestRunStepAttachments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "TestRuns",
                newName: "TestPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_TestRuns_PlanId",
                table: "TestRuns",
                newName: "IX_TestRuns_TestPlanId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TestRunSteps",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TestRunSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TestRunSteps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TestRunSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TestRunSteps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TestRunSteps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TestRunSteps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "TestRunStepAttachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TestRunStepAttachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TestRunStepAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TestRunStepAttachments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "TestRunStepAttachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TestRunStepAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "TestRunStepAttachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TestRunStepAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExecutedByUserId",
                table: "TestRuns",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TestRuns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TestRuns",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TestRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "TestRuns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TestRuns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TestRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "TestRuns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_ExecutedByUserId",
                table: "TestRuns",
                column: "ExecutedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_AspNetUsers_ExecutedByUserId",
                table: "TestRuns",
                column: "ExecutedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_TestPlans_TestPlanId",
                table: "TestRuns",
                column: "TestPlanId",
                principalTable: "TestPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_AspNetUsers_ExecutedByUserId",
                table: "TestRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_TestRuns_TestPlans_TestPlanId",
                table: "TestRuns");

            migrationBuilder.DropIndex(
                name: "IX_TestRuns_ExecutedByUserId",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TestRunSteps");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TestRunSteps");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TestRunSteps");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TestRunSteps");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TestRunSteps");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TestRunSteps");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TestRunSteps");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TestRunStepAttachments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TestRuns");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TestRuns");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "TestRunStepAttachments",
                newName: "FileType");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TestRunStepAttachments",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "TestPlanId",
                table: "TestRuns",
                newName: "PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_TestRuns_TestPlanId",
                table: "TestRuns",
                newName: "IX_TestRuns_PlanId");

            migrationBuilder.AddColumn<long>(
                name: "FileSizeInBytes",
                table: "TestRunStepAttachments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExecutedByUserId",
                table: "TestRuns",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutedById",
                table: "TestRuns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_ExecutedById",
                table: "TestRuns",
                column: "ExecutedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_AspNetUsers_ExecutedById",
                table: "TestRuns",
                column: "ExecutedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestRuns_TestPlans_PlanId",
                table: "TestRuns",
                column: "PlanId",
                principalTable: "TestPlans",
                principalColumn: "Id");
        }
    }
}
