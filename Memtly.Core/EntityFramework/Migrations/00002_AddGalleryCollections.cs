using Microsoft.EntityFrameworkCore.Migrations;
using Memtly.Core.Enums;

namespace Memtly.Core.EntityFramework.Migrations
{
    public partial class AddGalleryCollections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// GALLERIES

            migrationBuilder.AddColumn<GalleryType>(name: "Type", table: "Galleries", nullable: false, defaultValue: GalleryType.Basic);

            /// GALLERY COLLECTIONS

            migrationBuilder.CreateTable(
                name: "GalleryCollections",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(nullable: false),
                    GalleryId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryCollections_Collection_CollectionId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryCollections_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GalleryCollections");
            migrationBuilder.DropColumn(name: "Type", table: "GalleryCollections");
        }
    }
}