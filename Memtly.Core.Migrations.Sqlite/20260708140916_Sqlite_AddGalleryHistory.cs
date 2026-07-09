using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Sqlite_AddGalleryHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    GalleryId = table.Column<int>(type: "INTEGER", nullable: true),
                    SecretKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryHistory_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GalleryHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryHistory_GalleryId",
                table: "GalleryHistory",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryHistory_Id",
                table: "GalleryHistory",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GalleryHistory_UserId_GalleryId",
                table: "GalleryHistory",
                columns: new[] { "UserId", "GalleryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryHistory");
        }
    }
}
