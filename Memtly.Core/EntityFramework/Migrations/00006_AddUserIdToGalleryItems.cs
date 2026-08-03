using Microsoft.EntityFrameworkCore.Migrations;

namespace Memtly.Core.EntityFramework.Migrations
{
    public partial class AddUserIdToGalleryItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// GALLERY ITEMS

            migrationBuilder.AddColumn<int?>(
                name: "UserId",
                table: "GalleryItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GalleryItems");
        }
    }
}