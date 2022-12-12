using Domain;

namespace GameBrain;

record MoveWithHeuristic(Move Move, int Heuristic) : IComparable<MoveWithHeuristic>
{
    public int CompareTo(MoveWithHeuristic? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Heuristic.CompareTo(other.Heuristic);
    }
}

public class CheckersAiSimple : AbstractCheckersAi
{
    public override EAiType AiType => EAiType.Simple;

    private int GetMoveHeuristic(Move move)
    {
        Console.WriteLine(move);
        Console.WriteLine(move.GamePieces.Count);
        return move.GamePieces.Count;
    }
    
    protected override Move? GetMove(CheckersBrain checkersBrain)
    {
        var availableMoves = checkersBrain.AvailableMoves();
        var movesWithHeuristic = new List<MoveWithHeuristic>();
        foreach (var move in availableMoves)
        {
            movesWithHeuristic.Add(new MoveWithHeuristic(move, GetMoveHeuristic(move)));
        }

        var random = new Random();
        var result = movesWithHeuristic.OrderBy(_ => random.Next()).Max()?.Move;
        Console.WriteLine(result);
        Console.WriteLine(result?.GamePieces.Count);
        return result;
    }
}