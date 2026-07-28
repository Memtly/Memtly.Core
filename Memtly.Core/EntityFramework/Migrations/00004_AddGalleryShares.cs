using Microsoft.EntityFrameworkCore.Migrations;

namespace Memtly.Core.EntityFramework.Migrations
{
    public partial class AddGalleryShares : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// GALLERY SHARES

            migrationBuilder.CreateTable(
                name: "GalleryShares",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    GalleryId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryShares_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryShares_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GalleryShares");
        }
    }
}