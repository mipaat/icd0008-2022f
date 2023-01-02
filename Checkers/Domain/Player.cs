using System.Text;
using Common;

namespace Domain;

public class Player
{
    public readonly CheckersGame CheckersGame;
    public readonly EPlayerColor Color;

    public Player(CheckersGame checkersGame, EPlayerColor color)
    {
        CheckersGame = checkersGame;
        Color = color;
    }

    public EAiType? AiType => Color switch
    {
        EPlayerColor.Black => CheckersGame.BlackAiType,
        EPlayerColor.White => CheckersGame.WhiteAiType,
        _ => throw new UnknownPlayerColorException(Color)
    };

    public bool IsAi => AiType != null;

    public string? Name => Color switch
    {
        EPlayerColor.Black => CheckersGame.BlackPlayerName,
        EPlayerColor.White => CheckersGame.WhitePlayerName,
        _ => throw new UnknownPlayerColorException(Color)
    };

    public override string ToString()
    {
        var result = new StringBuilder(Color.ToString());

        var extraInfo = new List<string>();
        if (Name != null && Name.Trim().Length > 0) extraInfo.Add($"Name: {Name}");
        if (IsAi) extraInfo.Add($"AI: {AiType}");

        if (extraInfo.Count > 0) result.Append(" (").AppendJoin(", ", extraInfo).Append(')');

        return result.ToString();
    }
}