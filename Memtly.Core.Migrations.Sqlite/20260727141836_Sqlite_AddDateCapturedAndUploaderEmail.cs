using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Sqlite_AddDateCapturedAndUploaderEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DateTaken",
                table: "GalleryItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploaderEmailAddress",
                table: "GalleryItems",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
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
