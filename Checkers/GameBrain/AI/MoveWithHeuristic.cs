namespace GameBrain.AI;

public record MoveWithHeuristic(Move Move, float Heuristic) : IComparable<MoveWithHeuristic>
{
    public int CompareTo(MoveWithHeuristic? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Heuristic.CompareTo(other.Heuristic);
    }
}