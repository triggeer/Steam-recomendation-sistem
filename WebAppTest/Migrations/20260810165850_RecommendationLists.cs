using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebAppTest.Migrations
{
    /// <inheritdoc />
    public partial class RecommendationLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ListGames",
                columns: table => new
                {
                    ListId = table.Column<int>(type: "integer", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    GamePosition = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListGames", x => new { x.ListId, x.GameId });
                });

            migrationBuilder.CreateTable(
                name: "RecLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserUserLists",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ListId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUserLists", x => new { x.UserId, x.ListId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_ListGames_ListId_GamePosition",
                table: "ListGames",
                columns: new[] { "ListId", "GamePosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecLists_Id",
                table: "RecLists",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListGames");

            migrationBuilder.DropTable(
                name: "RecLists");

            migrationBuilder.DropTable(
                name: "UserUserLists");
        }
    }
}
