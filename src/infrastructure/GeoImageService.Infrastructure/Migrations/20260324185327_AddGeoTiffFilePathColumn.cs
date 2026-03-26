using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoImageService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoTiffFilePathColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "geo_tiff_file_path",
                table: "images",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "geo_tiff_file_path",
                table: "images");
        }
    }
}
