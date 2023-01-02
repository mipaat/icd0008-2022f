using Common;
using Domain;

namespace GameBrain.AI;

public interface ICheckersAi
{
    public EAiType AiType { get; }

    public void Move(CheckersBrain checkersBrain);
}