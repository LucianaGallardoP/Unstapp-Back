using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddYearToSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "Schedules");
        }
    }
}
