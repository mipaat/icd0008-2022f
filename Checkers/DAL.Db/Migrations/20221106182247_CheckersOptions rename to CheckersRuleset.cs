using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    public partial class CheckersOptionsrenametoCheckersRuleset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckersGames_CheckersOptions_CheckersOptionsId",
                table: "CheckersGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CheckersOptions",
                table: "CheckersOptions");

            migrationBuilder.RenameTable(
                name: "CheckersOptions",
                newName: "CheckersRulesets");

            migrationBuilder.RenameColumn(
                name: "CheckersOptionsId",
                table: "CheckersGames",
                newName: "CheckersRulesetId");

            migrationBuilder.RenameIndex(
                name: "IX_CheckersGames_CheckersOptionsId",
                table: "CheckersGames",
                newName: "IX_CheckersGames_CheckersRulesetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheckersRulesets",
                table: "CheckersRulesets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckersGames_CheckersRulesets_CheckersRulesetId",
                table: "CheckersGames",
                column: "CheckersRulesetId",
                principalTable: "CheckersRulesets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckersGames_CheckersRulesets_CheckersRulesetId",
                table: "CheckersGames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CheckersRulesets",
                table: "CheckersRulesets");

            migrationBuilder.RenameTable(
                name: "CheckersRulesets",
                newName: "CheckersOptions");

            migrationBuilder.RenameColumn(
                name: "CheckersRulesetId",
                table: "CheckersGames",
                newName: "CheckersOptionsId");

            migrationBuilder.RenameIndex(
                name: "IX_CheckersGames_CheckersRulesetId",
                table: "CheckersGames",
                newName: "IX_CheckersGames_CheckersOptionsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CheckersOptions",
                table: "CheckersOptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckersGames_CheckersOptions_CheckersOptionsId",
                table: "CheckersGames",
                column: "CheckersOptionsId",
                principalTable: "CheckersOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
