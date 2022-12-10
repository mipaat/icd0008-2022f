using Domain;

namespace GameBrain;

public abstract class AbstractCheckersAi : ICheckersAi
{
    public abstract EAiType AiType { get; }

    protected abstract Move GetMove(CheckersBrain checkersBrain);

    public void Move(CheckersBrain checkersBrain)
    {
        var move = GetMove(checkersBrain);
        checkersBrain.Move(move.FromX, move.FromY, move.ToX, move.ToY);
    }
}