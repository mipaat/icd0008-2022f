using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddAItyperemoveelapsedtimetracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameElapsedTime",
                table: "CheckersStates");

            migrationBuilder.DropColumn(
                name: "MoveElapsedTime",
                table: "CheckersStates");

            migrationBuilder.AddColumn<int>(
                name: "BlackAiType",
                table: "CheckersGames",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WhiteAiType",
                table: "CheckersGames",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlackAiType",
                table: "CheckersGames");

            migrationBuilder.DropColumn(
                name: "WhiteAiType",
                table: "CheckersGames");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GameElapsedTime",
                table: "CheckersStates",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MoveElapsedTime",
                table: "CheckersStates",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
