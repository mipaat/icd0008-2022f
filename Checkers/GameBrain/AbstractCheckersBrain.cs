using Domain;

namespace GameBrain;

public abstract class AbstractCheckersBrain
{
    public abstract int Width { get; }
    public abstract int Height { get; }
    public abstract bool IsSquareBlack(int x, int y);

    public abstract GamePiece? this[int x, int y] { get; }

    public abstract EPlayerColor PlayerColor(string playerId);

    public abstract DateTime StartedAt { get; set; }
    public abstract DateTime? EndedAt { get; set; }
    public abstract TimeSpan MoveElapsedTime { get; set; }
    public abstract TimeSpan GameElapsedTime { get; set; }
    
    public abstract int WhiteMoves { get; set; }
    public abstract int BlackMoves { get; set; }
    
    public abstract bool IsMoveValid(int sourceX, int sourceY, int destinationX, int destinationY);
    public abstract EMoveState Move(int sourceX, int sourceY, int destinationX, int destinationY);

    public abstract CheckersGame GetSaveGameState();
}