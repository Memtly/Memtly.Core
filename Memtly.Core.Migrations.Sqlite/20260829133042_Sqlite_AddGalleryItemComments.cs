using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Sqlite_AddGalleryItemComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GalleryItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    Value = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: false),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryComments_GalleryItems_GalleryItemId",
                        column: x => x.GalleryItemId,
                        principalTable: "GalleryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryComments_GalleryItemId",
                table: "GalleryComments",
                column: "GalleryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryComments_UserId",
                table: "GalleryComments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryComments");
        }
    }
}
