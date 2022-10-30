using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    public partial class InitialDatabaseCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckersOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BuiltIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    Saved = table.Column<bool>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    BlackMovesFirst = table.Column<bool>(type: "INTEGER", nullable: false),
                    MustCapture = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanJumpBackwards = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanJumpBackwardsDuringMultiCapture = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckersOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CheckersGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WhitePlayerId = table.Column<string>(type: "TEXT", nullable: true),
                    BlackPlayerId = table.Column<string>(type: "TEXT", nullable: true),
                    CheckersOptionsId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Winner = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckersGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckersGames_CheckersOptions_CheckersOptionsId",
                        column: x => x.CheckersOptionsId,
                        principalTable: "CheckersOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckersStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CheckersGameId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SerializedGamePieces = table.Column<string>(type: "TEXT", nullable: false),
                    MoveElapsedTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    GameElapsedTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    WhiteMoves = table.Column<int>(type: "INTEGER", nullable: false),
                    BlackMoves = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckersStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckersStates_CheckersGames_CheckersGameId",
                        column: x => x.CheckersGameId,
                        principalTable: "CheckersGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckersGames_CheckersOptionsId",
                table: "CheckersGames",
                column: "CheckersOptionsId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckersStates_CheckersGameId",
                table: "CheckersStates",
                column: "CheckersGameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckersStates");

            migrationBuilder.DropTable(
                name: "CheckersGames");

            migrationBuilder.DropTable(
                name: "CheckersOptions");
        }
    }
}
