using Domain;

namespace DAL;

public static class DefaultCheckersRulesets
{
    public static List<CheckersRuleset> DefaultRulesets => new()
    {
        new CheckersRuleset
        {
            Title = "Classic (8x8)",
            BuiltIn = true,
        },
        new CheckersRuleset
        {
            Title = "10x10",
            BuiltIn = true,
            Width = 10,
            Height = 10,
        },
        new CheckersRuleset
        {
            Title = "No captures required",
            BuiltIn = true,
            MustCapture = false
        },
        new CheckersRuleset
        {
            Title = "Backwards jumps allowed",
            BuiltIn = true,
            CanJumpBackwards = true,
            CanJumpBackwardsDuringMultiCapture = true
        }
    };
}