using Microsoft.EntityFrameworkCore.Migrations;

namespace Memtly.Core.EntityFramework.Migrations
{
    public partial class AddGalleryHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// GALLERY HISTORY

            migrationBuilder.CreateTable(
                name: "GalleryHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(nullable: false),
                    GalleryId = table.Column<int>(nullable: false),
                    SecretKey = table.Column<string>(maxLength: 500, nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryHistory_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GalleryHistory");
        }
    }
}