using Microsoft.EntityFrameworkCore.Migrations;

namespace Memtly.Core.EntityFramework.Migrations
{
    public partial class AddGalleryItemComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// GALLERY COMMENTS

            migrationBuilder.CreateTable(
                name: "GalleryComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                        .Annotation("Sqlite:Autoincrement", true),
                    GalleryId = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 5000, nullable: false),
                    CreatedAt = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryItemComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryItemComment_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryItemComment_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GalleryComments");
        }
    }
}