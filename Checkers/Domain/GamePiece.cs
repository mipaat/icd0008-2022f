using Common;

namespace Domain;

public struct GamePiece
{
    public EPlayerColor Player;
    public bool IsCrowned;

    public GamePiece(EPlayerColor player, bool isCrowned = false)
    {
        Player = player;
        IsCrowned = isCrowned;
    }
}