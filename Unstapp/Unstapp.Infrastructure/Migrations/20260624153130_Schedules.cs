using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Unstapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Schedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CareerId = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DurationHours = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    Professor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Classroom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Careers_CareerId",
                        column: x => x.CareerId,
                        principalTable: "Careers",
                        principalColumn: "CareerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CareerId",
                table: "Schedules",
                column: "CareerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}
