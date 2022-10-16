using Domain;

namespace GameBrain;

public abstract class AbstractCheckersBrain
{
    public abstract int Width { get; }
    public abstract int Height { get; }
    public abstract bool IsSquareBlack(int x, int y);

    public abstract GamePiece? this[int x, int y] { get; }

    public abstract EPlayerColor PlayerColor(string playerId);

    public abstract bool IsMoveValid(int sourceX, int sourceY, int destinationX, int destinationY);
}