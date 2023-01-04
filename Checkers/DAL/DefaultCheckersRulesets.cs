using Domain.Model;

namespace DAL;

public static class DefaultCheckersRulesets
{
    public static List<CheckersRuleset> DefaultRulesets => new()
    {
        new CheckersRuleset
        {
            Title = "Classic (8x8)",
            BuiltIn = true
        },
        new CheckersRuleset
        {
            Title = "Classic (10x10)",
            BuiltIn = true,
            Width = 10,
            Height = 10
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
            CanCaptureBackwards = true,
            CanCaptureBackwardsDuringMultiCapture = true
        },
        new CheckersRuleset
        {
            Title = "Freedom",
            BuiltIn = true,
            CanCaptureBackwards = true,
            CanCaptureBackwardsDuringMultiCapture = true,
            MustCapture = false,
            FlyingKings = true
        },
        new CheckersRuleset
        {
            Title = "Classic MINI",
            BuiltIn = true,
            Width = 4,
            Height = 4
        },
        new CheckersRuleset
        {
            Title = "Classic MEGA",
            BuiltIn = true,
            Width = 20,
            Height = 20
        }
    };
}