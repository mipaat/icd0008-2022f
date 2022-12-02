using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    public partial class RecordnecessarypreviousmovedatainCheckersState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastMoveState",
                table: "CheckersStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastMovedToX",
                table: "CheckersStates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastMovedToY",
                table: "CheckersStates",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMoveState",
                table: "CheckersStates");

            migrationBuilder.DropColumn(
                name: "LastMovedToX",
                table: "CheckersStates");

            migrationBuilder.DropColumn(
                name: "LastMovedToY",
                table: "CheckersStates");
        }
    }
}
