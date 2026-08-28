using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppTest.Migrations
{
    /// <inheritdoc />
    public partial class RecommendationLists1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserUserLists",
                table: "UserUserLists");

            migrationBuilder.RenameTable(
                name: "UserUserLists",
                newName: "UserLists");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLists",
                table: "UserLists",
                columns: new[] { "UserId", "ListId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLists",
                table: "UserLists");

            migrationBuilder.RenameTable(
                name: "UserLists",
                newName: "UserUserLists");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserUserLists",
                table: "UserUserLists",
                columns: new[] { "UserId", "ListId" });
        }
    }
}
