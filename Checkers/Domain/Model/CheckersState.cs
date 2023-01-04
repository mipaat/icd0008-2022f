using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Model.Helpers;

namespace Domain.Model;

public class CheckersState : AbstractDatabaseEntity
{
    [DisplayName("CheckersGame ID")]
    public int CheckersGameId { get; set; }

    [DisplayName("Created at")] public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

    [ExpectedNotNull]
    [JsonIgnore]
    [NotMapped]
    public GamePiece?[,]? GamePieces { get; set; }

    [DisplayName("Game pieces (serialized)")]
    public string SerializedGamePieces { get; set; } = default!;

    [DisplayName("White moves")] public int WhiteMoves { get; set; }
    [DisplayName("Black moves")] public int BlackMoves { get; set; }

    [DisplayName("Last move end X")]
    [PreferNotNull]
    public int? LastMovedToX { get; set; }

    [DisplayName("Last move end Y")]
    [PreferNotNull]
    public int? LastMovedToY { get; set; }

    [DisplayName("Last move end state")]
    [PreferNotNull] public EMoveState? LastMoveState { get; set; }

    public void SerializeGamePieces()
    {
        var compressedGamePieces = new CompressedGamePieces(GamePieces!.GetLength(0), GamePieces.GetLength(1));
        for (var y = 0; y < GamePieces.GetLength(1); y++)
        for (var x = 0; x < GamePieces.GetLength(0); x++)
        {
            var gamePiece = GamePieces[x, y];
            if (gamePiece == null) continue;
            var actualGamePiece = (GamePiece)gamePiece;
            compressedGamePieces.GamePiecesWithPosition.Add(new CompressedGamePieces.GamePieceWithPosition
            {
                X = x,
                Y = y,
                Player = actualGamePiece.Player,
                IsCrowned = actualGamePiece.IsCrowned
            });
        }

        SerializedGamePieces = JsonSerializer.Serialize(compressedGamePieces);
    }

    public void DeserializeGamePieces()
    {
        var compressedGamePieces = JsonSerializer.Deserialize<CompressedGamePieces>(SerializedGamePieces);
        if (compressedGamePieces == null)
            throw new Exception($"Couldn't deserialize CompressedGamePieces from '{SerializedGamePieces}'!");
        GamePieces = new GamePiece?[compressedGamePieces.X, compressedGamePieces.Y];
        foreach (var gamePieceWithPosition in compressedGamePieces.GamePiecesWithPosition)
            GamePieces[gamePieceWithPosition.X, gamePieceWithPosition.Y] =
                new GamePiece(gamePieceWithPosition.Player, gamePieceWithPosition.IsCrowned);
    }

    private record CompressedGamePieces(int X, int Y)
    {
        public List<GamePieceWithPosition> GamePiecesWithPosition { get; init; } = new();

        public record GamePieceWithPosition
        {
            public int X { get; init; }
            public int Y { get; init; }
            public EPlayerColor Player { get; init; }
            public bool IsCrowned { get; init; }
        }
    }
}