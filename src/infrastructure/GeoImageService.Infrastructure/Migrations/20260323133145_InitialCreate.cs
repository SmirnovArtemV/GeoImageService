using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GeoImageService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    corners_coordinates_top_left_latitude = table.Column<double>(type: "double precision", nullable: false),
                    corners_coordinates_top_left_longitude = table.Column<double>(type: "double precision", nullable: false),
                    corners_coordinates_top_right_latitude = table.Column<double>(type: "double precision", nullable: false),
                    corners_coordinates_top_right_longitude = table.Column<double>(type: "double precision", nullable: false),
                    corners_coordinates_bottom_right_latitude = table.Column<double>(type: "double precision", nullable: false),
                    corners_coordinates_bottom_right_longitude = table.Column<double>(type: "double precision", nullable: false),
                    corners_coordinates_bottom_left_latitude = table.Column<double>(type: "double precision", nullable: false),
                    corners_coordinates_bottom_left_longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_images", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "images");
        }
    }
}
