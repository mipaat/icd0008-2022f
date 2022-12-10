using Domain;

namespace GameBrain;

public class CheckersAiRandom : AbstractCheckersAi
{
    public override EAiType AiType => EAiType.Random;

    protected override Move GetMove(CheckersBrain checkersBrain)
    {
        var availableMoves = checkersBrain.AvailableMoves();
        if (availableMoves.Count == 0)
            throw new IllegalStateException("No moves available, but move was expected from AI!");
        var randomIndex = Random.Shared.Next(availableMoves.Count);
        return availableMoves[randomIndex];
    }
}