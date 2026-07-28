using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class SqlServer_AddGalleryShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryShare",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    GalleryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryShare", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryShare_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GalleryShare_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryShare_GalleryId",
                table: "GalleryShare",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryShare_Id",
                table: "GalleryShare",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GalleryShare_UserId_GalleryId",
                table: "GalleryShare",
                columns: new[] { "UserId", "GalleryId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [GalleryId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryShare");
        }
    }
}
