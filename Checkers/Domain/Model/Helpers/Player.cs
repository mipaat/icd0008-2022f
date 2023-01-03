using System.Text;

namespace Domain.Model.Helpers;

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

    public int Id => GetId(Color);

    public bool IsAi => AiType != null;

    public string? Name => Color switch
    {
        EPlayerColor.Black => CheckersGame.BlackPlayerName,
        EPlayerColor.White => CheckersGame.WhitePlayerName,
        _ => throw new UnknownPlayerColorException(Color)
    };

    public static int GetId(EPlayerColor playerColor)
    {
        return playerColor switch
        {
            EPlayerColor.Black => 0,
            EPlayerColor.White => 1,
            _ => throw new UnknownPlayerColorException(playerColor)
        };
    }

    public static EPlayerColor GetPlayerColor(int id)
    {
        return id switch
        {
            0 => EPlayerColor.Black,
            1 => EPlayerColor.White,
            _ => throw new ArgumentOutOfRangeException($"Argument {nameof(id)}: {id} out of range (0-1)!")
        };
    }

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