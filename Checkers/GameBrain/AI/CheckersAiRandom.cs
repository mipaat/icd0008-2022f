using Domain;

namespace GameBrain.AI;

public class CheckersAiRandom : AbstractCheckersAiHeuristic
{
    public override EAiType AiType => EAiType.Random;

    protected override float GetMoveHeuristic(CheckersBrain checkersBrain, Move move)
    {
        return Random.Next();
    }
}