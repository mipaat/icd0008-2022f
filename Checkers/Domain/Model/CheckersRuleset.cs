using System.ComponentModel;
using System.Text;

namespace Domain.Model;

public class CheckersRuleset : AbstractDatabaseEntity, ICloneable
{
    private const int MinDimension = 4;
    private int _height = 8;

    private string? _title;
    private int _width = 8;
    [DisplayName("Built-in")] public bool BuiltIn { get; init; }
    public bool Saved { get; init; } = true;

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

    public string? Title
    {
        get => _title;
        set
        {
            if (value?.Trim().Length > 0) _title = value;
        }
    }

    public string TitleText => Title ?? $"No title! ({Width}x{Height})";
    public string? Description { get; set; }
    [DisplayName("Created at")] public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    [DisplayName("Last modified")] public DateTime LastModified { get; set; } = DateTime.Now.ToUniversalTime();

    [DisplayName("First move by black")] public bool BlackMovesFirst { get; set; } = true;

    [DisplayName("Must capture if possible")]
    public bool MustCapture { get; set; } = true;

    [DisplayName("Can capture backwards")] public bool CanCaptureBackwards { get; set; }

    [DisplayName("Can capture backwards during multi-capture")]
    public bool CanCaptureBackwardsDuringMultiCapture { get; set; }

    [DisplayName("Flying Kings")] public bool FlyingKings { get; set; }

    public object Clone()
    {
        return GetClone();
    }

    public bool IsEditable => Saved && !BuiltIn;

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

    public override string ToString()
    {
        var result = new StringBuilder();
        var outerContents = new List<string>();
        var contents = new List<string>();

        if (Title != null) outerContents.Add(Title);

        var dimensionsString = $"{Width}x{Height}";
        if (!(Title?.Replace(" ", "").ToLower().Contains(dimensionsString) ?? false))
            contents.Add(dimensionsString);
        if (!MustCapture) contents.Add("Captures not required");
        if (CanCaptureBackwards)
            contents.Add("Backwards captures");
        else if (CanCaptureBackwardsDuringMultiCapture) contents.Add("Backwards during multi-capture");
        if (FlyingKings) contents.Add("Flying Kings");

        if (contents.Count > 0) outerContents.Add(string.Join(", ", contents));
        result.AppendJoin(" - ", outerContents);
        return result.ToString();
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
            MustCapture = MustCapture,
            CanCaptureBackwards = CanCaptureBackwards,
            CanCaptureBackwardsDuringMultiCapture = CanCaptureBackwardsDuringMultiCapture,
            FlyingKings = FlyingKings,
            Title = Title,
            Description = Description
        };
    }

    public void UpdateLastModified()
    {
        LastModified = DateTime.Now.ToUniversalTime();
    }
}