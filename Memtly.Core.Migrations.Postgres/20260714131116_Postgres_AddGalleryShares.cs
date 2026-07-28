using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Memtly.Core.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Postgres_AddGalleryShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GalleryShare",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    GalleryId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
