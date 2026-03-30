using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoImageService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "time_stamps_end",
                table: "images",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "time_stamps_start",
                table: "images",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "time_stamps_end",
                table: "images");

            migrationBuilder.DropColumn(
                name: "time_stamps_start",
                table: "images");
        }
    }
}
