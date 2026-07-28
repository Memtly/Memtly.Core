using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class SqlServer_UpdateDatabaseScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GalleryCollections_Galleries_CollectionId",
                table: "GalleryCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryCollections_Galleries_GalleryId",
                table: "GalleryCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryHistory_Galleries_GalleryId",
                table: "GalleryHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryHistory_Users_UserId",
                table: "GalleryHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryShare_Galleries_GalleryId",
                table: "GalleryShare");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryShare_Users_UserId",
                table: "GalleryShare");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryCollections_Galleries_CollectionId",
                table: "GalleryCollections",
                column: "CollectionId",
                principalTable: "Galleries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryCollections_Galleries_GalleryId",
                table: "GalleryCollections",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryHistory_Galleries_GalleryId",
                table: "GalleryHistory",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryHistory_Users_UserId",
                table: "GalleryHistory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryShare_Galleries_GalleryId",
                table: "GalleryShare",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryShare_Users_UserId",
                table: "GalleryShare",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GalleryCollections_Galleries_CollectionId",
                table: "GalleryCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryCollections_Galleries_GalleryId",
                table: "GalleryCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryHistory_Galleries_GalleryId",
                table: "GalleryHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryHistory_Users_UserId",
                table: "GalleryHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryShare_Galleries_GalleryId",
                table: "GalleryShare");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryShare_Users_UserId",
                table: "GalleryShare");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryCollections_Galleries_CollectionId",
                table: "GalleryCollections",
                column: "CollectionId",
                principalTable: "Galleries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryCollections_Galleries_GalleryId",
                table: "GalleryCollections",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryHistory_Galleries_GalleryId",
                table: "GalleryHistory",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryHistory_Users_UserId",
                table: "GalleryHistory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryShare_Galleries_GalleryId",
                table: "GalleryShare",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryShare_Users_UserId",
                table: "GalleryShare",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
