using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppTest.Migrations
{
    /// <inheritdoc />
    public partial class AccessModifierOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Id",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "playTime",
                table: "UserGames",
                newName: "PlayTime");

            migrationBuilder.RenameColumn(
                name: "gameId",
                table: "UserGames",
                newName: "GameId");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "UserGames",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "InitialPrice",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserProfiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PlayTime",
                table: "UserGames",
                newName: "playTime");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "UserGames",
                newName: "gameId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserGames",
                newName: "userId");

            migrationBuilder.AlterColumn<int>(
                name: "InitialPrice",
                table: "Games",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Id",
                table: "UserProfiles",
                column: "Id",
                unique: true);
        }
    }
}
