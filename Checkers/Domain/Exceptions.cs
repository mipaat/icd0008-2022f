using Common;

namespace Domain;

public class InsufficientCheckersStatesException : ArgumentException
{
    private readonly CheckersGame _checkersGame;
    
    public InsufficientCheckersStatesException(CheckersGame checkersGame)
    {
        _checkersGame = checkersGame;
    }

    public override string Message
    {
        get
        {
            var checkersStatesString = _checkersGame.CheckersStates == null
                ? "NULL"
                : _checkersGame.CheckersStates.Count.ToString();
            return $"{_checkersGame} has {checkersStatesString} checkersStates";
        }
    }
}


public class UnknownPlayerColorException : ArgumentException
{
    public UnknownPlayerColorException(EPlayerColor? playerColor) : base(
        $"Unknown player color '{playerColor.ToString() ?? "NULL"}'")
    {
    }
}