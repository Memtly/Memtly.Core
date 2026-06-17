using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Sqlite_AddGalleryCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Galleries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "GalleryCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: true),
                    GalleryId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryCollections_Galleries_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Galleries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GalleryCollections_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryCollections_CollectionId_GalleryId",
                table: "GalleryCollections",
                columns: new[] { "CollectionId", "GalleryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GalleryCollections_GalleryId",
                table: "GalleryCollections",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryCollections_Id",
                table: "GalleryCollections",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryCollections");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Galleries");
        }
    }
}
