namespace Game;

public struct GamePiece
{
    public readonly int InitialX, InitialY;
    public EPlayerColor Player;
    public bool IsCrowned;

    public GamePiece(int initialX, int initialY, EPlayerColor player, bool isCrowned = false)
    {
        InitialX = initialX;
        InitialY = initialY;
        Player = player;
        IsCrowned = isCrowned;
    }
}