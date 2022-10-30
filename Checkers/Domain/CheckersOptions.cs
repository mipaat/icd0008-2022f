namespace Domain;

public class CheckersOptions : AbstractDatabaseEntity
{
    public bool BuiltIn { get; init; }
    public bool Saved { get; init; } = true;
    private int _width = 8;
    private int _height = 8;

    public int Width
    {
        get => _width;
        set
        {
            if (!IsDimensionValid(value, out var feedbackMessage))
                throw new ArgumentOutOfRangeException(feedbackMessage);
            _width = value;
        }
    }

    public int Height
    {
        get => _height;
        set
        {
            if (!IsDimensionValid(value, out var feedbackMessage))
                throw new ArgumentOutOfRangeException(feedbackMessage);
            _height = value;
        }
    }

    public string Title { get; set; } = "No Option Title";
    public string? Description { get; set; }

    public bool BlackMovesFirst { get; set; } = true;
    public bool MustCapture { get; set; } = true;
    public bool CanJumpBackwards { get; set; }
    public bool CanJumpBackwardsDuringMultiCapture { get; set; }

    private const int MinDimension = 4;

    public static bool IsDimensionValid(int dimension)
    {
        return IsDimensionValid(dimension, out _);
    }

    public static bool IsDimensionValid(int dimension, out string? feedbackMessage)
    {
        if (dimension < MinDimension)
        {
            feedbackMessage = $"Checkers Board dimension must be at least {MinDimension}! (Was: {dimension})";
            return false;
        }

        feedbackMessage = null;
        return true;
    }

    public bool IsEquivalent(CheckersOptions other)
    {
        return other.Width == Width && other.Height == Height && other.BlackMovesFirst == BlackMovesFirst &&
               other.MustCapture == MustCapture && other.CanJumpBackwards == CanJumpBackwards &&
               other.CanJumpBackwardsDuringMultiCapture == CanJumpBackwardsDuringMultiCapture &&
               other.BuiltIn == BuiltIn && other.Saved == Saved && other.Title == Title;
    }
}