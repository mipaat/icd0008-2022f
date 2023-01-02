using Common;
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

        if (myPieces == 0) return float.MinValue;
        if (opponentPieces == 0) return float.MaxValue;
        return myPieces * 1.5f - opponentPieces;
    }
}