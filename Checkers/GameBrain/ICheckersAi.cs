using Domain;

namespace GameBrain;

public interface ICheckersAi
{
    public EAiType AiType { get; }

    public void Move(CheckersBrain checkersBrain);
}