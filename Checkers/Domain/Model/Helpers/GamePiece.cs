namespace Domain.Model.Helpers;

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