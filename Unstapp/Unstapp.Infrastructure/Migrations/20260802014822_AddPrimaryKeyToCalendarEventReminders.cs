using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrimaryKeyToCalendarEventReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CalendarEventReminders_CalendarEventReminderId",
                table: "CalendarEventReminders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CalendarEventReminders_CalendarEventReminderId",
                table: "CalendarEventReminders",
                column: "CalendarEventReminderId");
        }
    }
}
