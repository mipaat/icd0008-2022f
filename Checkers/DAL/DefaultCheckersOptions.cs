using Domain;

namespace DAL;

public class DefaultCheckersOptions
{
    public static List<CheckersOptions> DefaultOptions => new()
    {
        new CheckersOptions
        {
            Title = "Classic (8x8)",
            BuiltIn = true,
        },
        new CheckersOptions
        {
            Title = "10x10",
            BuiltIn = true,
            Width = 10,
            Height = 10,
        },
        new CheckersOptions
        {
            Title = "No captures required",
            BuiltIn = true,
            MustCapture = false
        },
        new CheckersOptions
        {
            Title = "Backwards jumps allowed",
            BuiltIn = true,
            CanJumpBackwards = true,
            CanJumpBackwardsDuringMultiCapture = true
        }
    };
}