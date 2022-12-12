using Domain;

namespace GameBrain.AI;

public class CheckersAiMinMaxSimple : AbstractCheckersAiMinMax
{
    public override EAiType AiType => EAiType.SimpleMinMax;
    protected override float GetGameStateHeuristic(CheckersBrain checkersBrain, EPlayerColor playerColor)
    {
        var pieceCounts = checkersBrain.PieceCounts;
        var myPieces = playerColor == EPlayerColor.Black ? pieceCounts.BlackPieces : pieceCounts.WhitePieces;
        var opponentPieces = playerColor == EPlayerColor.Black ? pieceCounts.WhitePieces : pieceCounts.BlackPieces;

        return myPieces * 1.5f - opponentPieces;
    }
}