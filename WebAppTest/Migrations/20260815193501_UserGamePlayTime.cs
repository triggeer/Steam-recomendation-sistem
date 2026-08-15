using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppTest.Migrations
{
    /// <inheritdoc />
    public partial class UserGamePlayTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "playTime",
                table: "UserGames",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "playTime",
                table: "UserGames");
        }
    }
}
