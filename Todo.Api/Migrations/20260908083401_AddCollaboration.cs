using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace todo.api.Migrations
{
    /// <inheritdoc />
    public partial class AddCollaboration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedByUserId",
                table: "TodoSteps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedByUserId",
                table: "TodoItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "TodoItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SharedCode",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FriendId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TodoShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TodoItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoShares_TodoItems_TodoItemId",
                        column: x => x.TodoItemId,
                        principalTable: "TodoItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TodoShares_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoSteps_CompletedByUserId",
                table: "TodoSteps",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CompletedByUserId",
                table: "TodoItems",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_FriendId",
                table: "Friendships",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserId",
                table: "Friendships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoShares_TodoItemId",
                table: "TodoShares",
                column: "TodoItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoShares_UserId",
                table: "TodoShares",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_Users_CompletedByUserId",
                table: "TodoItems",
                column: "CompletedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoSteps_Users_CompletedByUserId",
                table: "TodoSteps",
                column: "CompletedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_Users_CompletedByUserId",
                table: "TodoItems");

            migrationBuilder.DropForeignKey(
                name: "FK_TodoSteps_Users_CompletedByUserId",
                table: "TodoSteps");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "TodoShares");

            migrationBuilder.DropIndex(
                name: "IX_TodoSteps_CompletedByUserId",
                table: "TodoSteps");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CompletedByUserId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "TodoSteps");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "SharedCode",
                table: "TodoItems");
        }
    }
}
