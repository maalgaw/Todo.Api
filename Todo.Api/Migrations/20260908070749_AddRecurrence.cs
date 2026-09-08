using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace todo.api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "TodoItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecurrenceDaysOfWeek",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceEndDate",
                table: "TodoItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceInterval",
                table: "TodoItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceType",
                table: "TodoItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "RecurrenceDaysOfWeek",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "RecurrenceEndDate",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "RecurrenceInterval",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "RecurrenceType",
                table: "TodoItems");
        }
    }
}
