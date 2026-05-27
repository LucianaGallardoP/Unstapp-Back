using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserAvatarToUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.AddColumn<byte[]>(
                name: "Avatar",
                table: "Users",
                type: "bytea",
                nullable: true);
        }
    }
}
