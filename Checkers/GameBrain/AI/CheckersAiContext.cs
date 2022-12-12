using Domain;

namespace GameBrain.AI;

public static class CheckersAiContext
{
    private static readonly List<ICheckersAi> CheckersAis = new List<ICheckersAi>
    {
        new CheckersAiRandom(),
        new CheckersAiSimple()
    };

    public static ICheckersAi? GetCheckersAi(EAiType aiType)
    {
        return CheckersAis.Find(ai => ai.AiType == aiType);
    }
}