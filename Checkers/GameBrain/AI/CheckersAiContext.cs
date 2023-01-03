using Domain.Model.Helpers;

namespace GameBrain.AI;

public static class CheckersAiContext
{
    private static readonly List<ICheckersAi> CheckersAis = new()
    {
        new CheckersAiRandom(),
        new CheckersAiSimple(),
        new CheckersAiMinMaxSimple()
    };

    public static ICheckersAi? GetCheckersAi(EAiType aiType)
    {
        return CheckersAis.Find(ai => ai.AiType == aiType);
    }
}