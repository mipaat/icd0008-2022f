using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCheckersGamePlayerIdtoPlayerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhitePlayerId",
                table: "CheckersGames",
                newName: "WhitePlayerName");

            migrationBuilder.RenameColumn(
                name: "BlackPlayerId",
                table: "CheckersGames",
                newName: "BlackPlayerName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhitePlayerName",
                table: "CheckersGames",
                newName: "WhitePlayerId");

            migrationBuilder.RenameColumn(
                name: "BlackPlayerName",
                table: "CheckersGames",
                newName: "BlackPlayerId");
        }
    }
}
