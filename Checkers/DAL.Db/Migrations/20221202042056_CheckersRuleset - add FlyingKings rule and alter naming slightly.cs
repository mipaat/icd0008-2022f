using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    public partial class CheckersRulesetaddFlyingKingsruleandalternamingslightly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanJumpBackwardsDuringMultiCapture",
                table: "CheckersRulesets",
                newName: "CanCaptureBackwardsDuringMultiCapture");

            migrationBuilder.RenameColumn(
                name: "CanJumpBackwards",
                table: "CheckersRulesets",
                newName: "CanCaptureBackwards");

            migrationBuilder.AddColumn<bool>(
                name: "FlyingKings",
                table: "CheckersRulesets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlyingKings",
                table: "CheckersRulesets");

            migrationBuilder.RenameColumn(
                name: "CanCaptureBackwardsDuringMultiCapture",
                table: "CheckersRulesets",
                newName: "CanJumpBackwardsDuringMultiCapture");

            migrationBuilder.RenameColumn(
                name: "CanCaptureBackwards",
                table: "CheckersRulesets",
                newName: "CanJumpBackwards");
        }
    }
}
