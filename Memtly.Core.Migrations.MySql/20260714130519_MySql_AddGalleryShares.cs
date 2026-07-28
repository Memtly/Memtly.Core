using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Memtly.Core.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class MySql_AddGalleryShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryShare",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    GalleryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryShare");
        }
    }
}
