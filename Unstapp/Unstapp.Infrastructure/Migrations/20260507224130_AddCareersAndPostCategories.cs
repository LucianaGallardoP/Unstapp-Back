using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Unstapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCareersAndPostCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Faculty",
                columns: table => new
                {
                    FacultyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculty", x => x.FacultyId);
                });

            migrationBuilder.CreateTable(
                name: "Careers",
                columns: table => new
                {
                    CareerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CareerName = table.Column<string>(type: "text", nullable: false),
                    FacultyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Careers", x => x.CareerId);
                    table.ForeignKey(
                        name: "FK_Careers_Faculty_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculty",
                        principalColumn: "FacultyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostCareers",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    CareerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostCareers", x => new { x.PostId, x.CareerId });
                    table.ForeignKey(
                        name: "FK_PostCareers_Careers_CareerId",
                        column: x => x.CareerId,
                        principalTable: "Careers",
                        principalColumn: "CareerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostCareers_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCareers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CareerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCareers", x => new { x.UserId, x.CareerId });
                    table.ForeignKey(
                        name: "FK_UserCareers_Careers_CareerId",
                        column: x => x.CareerId,
                        principalTable: "Careers",
                        principalColumn: "CareerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCareers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Careers_FacultyId",
                table: "Careers",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_PostCareers_CareerId",
                table: "PostCareers",
                column: "CareerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCareers_CareerId",
                table: "UserCareers",
                column: "CareerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostCareers");

            migrationBuilder.DropTable(
                name: "UserCareers");

            migrationBuilder.DropTable(
                name: "Careers");

            migrationBuilder.DropTable(
                name: "Faculty");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Posts");
        }
    }
}
