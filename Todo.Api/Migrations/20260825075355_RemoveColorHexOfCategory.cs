using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace todo.api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveColorHexOfCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
