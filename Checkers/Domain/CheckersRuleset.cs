namespace Domain;

public class CheckersRuleset : AbstractDatabaseEntity, ICloneable
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

    private string? _title;
    public string? Title
    {
        get => _title;
        set
        {
            if (value?.Trim().Length > 0) _title = value;
        }
    }

    public string TitleText => Title ?? $"No title! ({Width}x{Height}, Modified: {LastModified.ToLocalTime()})";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime LastModified { get; set; } = DateTime.Now.ToUniversalTime();

    public bool BlackMovesFirst { get; set; } = true;
    public bool MustCapture { get; set; } = true;
    public bool CanCaptureBackwards { get; set; }
    public bool CanCaptureBackwardsDuringMultiCapture { get; set; }
    public bool FlyingKings { get; set; } = false;

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

    public bool IsEquivalent(CheckersRuleset other)
    {
        return other.Width == Width && other.Height == Height && other.BlackMovesFirst == BlackMovesFirst &&
               other.MustCapture == MustCapture && other.CanCaptureBackwards == CanCaptureBackwards &&
               other.CanCaptureBackwardsDuringMultiCapture == CanCaptureBackwardsDuringMultiCapture &&
               other.BuiltIn == BuiltIn && other.Saved == Saved && other.Title == Title
               && other.FlyingKings == FlyingKings;
    }

    public CheckersRuleset GetClone(bool? saved = null)
    {
        return new CheckersRuleset
        {
            Height = Height,
            Width = Width,
            BlackMovesFirst = BlackMovesFirst,
            BuiltIn = false,
            Saved = saved ?? Saved,
            CanCaptureBackwards = CanCaptureBackwards,
            CanCaptureBackwardsDuringMultiCapture = CanCaptureBackwardsDuringMultiCapture,
            FlyingKings = FlyingKings,
            Title = Title,
            Description = Description
        };
    }

    public object Clone()
    {
        return GetClone();
    }

    public void UpdateLastModified()
    {
        LastModified = DateTime.Now.ToUniversalTime();
    }
}