namespace GameBrain.AI;

public abstract class AbstractCheckersAiHeuristic : AbstractCheckersAi
{
    protected static readonly Random Random = new();
    protected abstract float GetMoveHeuristic(CheckersBrain checkersBrain, Move move);

    protected override Move? GetMove(CheckersBrain checkersBrain)
    {
        var availableMoves = checkersBrain.AvailableMoves();
        var movesWithHeuristic = new List<MoveWithHeuristic>();
        foreach (var move in availableMoves)
            movesWithHeuristic.Add(new MoveWithHeuristic(move, GetMoveHeuristic(checkersBrain, move)));

        return movesWithHeuristic.OrderBy(_ => Random.Next()).Max()?.Move;
    }
}