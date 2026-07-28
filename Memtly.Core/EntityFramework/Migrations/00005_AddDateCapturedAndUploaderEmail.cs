using Microsoft.EntityFrameworkCore.Migrations;

namespace Memtly.Core.EntityFramework.Migrations
{
    public partial class AddDateCapturedAndUploaderEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// GALLERY ITEMS

            migrationBuilder.AddColumn<long>(
                name: "DateTaken",
                table: "GalleryItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploaderEmailAddress",
                table: "GalleryItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTaken",
                table: "GalleryItems");

            migrationBuilder.DropColumn(
                name: "UploaderEmailAddress",
                table: "GalleryItems");
        }
    }
}