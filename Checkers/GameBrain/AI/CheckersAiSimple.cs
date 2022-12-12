using Domain;

namespace GameBrain.AI;

public class CheckersAiSimple : AbstractCheckersAiHeuristic
{
    public override EAiType AiType => EAiType.Simple;

    protected override float GetMoveHeuristic(CheckersBrain checkersBrain, Move move)
    {
        return move.GamePieces.Count;
    }
}