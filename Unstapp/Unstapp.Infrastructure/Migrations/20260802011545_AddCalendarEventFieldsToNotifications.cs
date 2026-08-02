using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Unstapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEventFieldsToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CalendarEventId",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CalendarEventId",
                table: "Notifications",
                column: "CalendarEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_CalendarEvents_CalendarEventId",
                table: "Notifications",
                column: "CalendarEventId",
                principalTable: "CalendarEvents",
                principalColumn: "CalendarEventId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_CalendarEvents_CalendarEventId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CalendarEventId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CalendarEventId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Notifications");
        }
    }
}
