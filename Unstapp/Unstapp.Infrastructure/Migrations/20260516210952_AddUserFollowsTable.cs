using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFollowsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFollow",
                columns: table => new
                {
                    FollowerUserId = table.Column<int>(type: "integer", nullable: false),
                    FollowedUserId = table.Column<int>(type: "integer", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollow", x => new { x.FollowerUserId, x.FollowedUserId });
                    table.ForeignKey(
                        name: "FK_UserFollow_Users_FollowedUserId",
                        column: x => x.FollowedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFollow_Users_FollowerUserId",
                        column: x => x.FollowerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollow_FollowedUserId",
                table: "UserFollow",
                column: "FollowedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFollow");
        }
    }
}
