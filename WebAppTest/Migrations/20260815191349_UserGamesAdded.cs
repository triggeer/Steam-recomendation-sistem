using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppTest.Migrations
{
    /// <inheritdoc />
    public partial class UserGamesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGames",
                columns: table => new
                {
                    userId = table.Column<string>(type: "text", nullable: false),
                    gameId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGames", x => new { x.userId, x.gameId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGames");
        }
    }
}
