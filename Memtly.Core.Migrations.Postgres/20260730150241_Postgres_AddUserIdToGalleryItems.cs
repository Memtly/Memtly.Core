using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Postgres_AddUserIdToGalleryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "GalleryItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_UserId",
                table: "GalleryItems",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryItems_Users_UserId",
                table: "GalleryItems",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GalleryItems_Users_UserId",
                table: "GalleryItems");

            migrationBuilder.DropIndex(
                name: "IX_GalleryItems_UserId",
                table: "GalleryItems");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GalleryItems");
        }
    }
}
