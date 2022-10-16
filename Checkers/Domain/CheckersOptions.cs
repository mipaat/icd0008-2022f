namespace Domain;

public class CheckersOptions : AbstractDatabaseEntity
{
    public bool BuiltIn { get; set; } = false;
    public bool Saved { get; set; } = true;
    public int Width { get; set; } = 8;
    public int Height { get; set; } = 8;

    public string Title { get; set; } = "No Option Title";
    public string? Description { get; set; } 

    public bool BlackMovesFirst { get; set; } = true;
    public bool MustCapture { get; set; } = true;
    public bool CanJumpBackwards { get; set; } = false;
    public bool CanJumpBackwardsDuringMultiCapture { get; set; } = false;

    public bool IsEquivalent(CheckersOptions other)
    {
        return other.Width == Width && other.Height == Height && other.BlackMovesFirst == BlackMovesFirst &&
               other.MustCapture == MustCapture && other.CanJumpBackwards == CanJumpBackwards &&
               other.CanJumpBackwardsDuringMultiCapture == CanJumpBackwardsDuringMultiCapture &&
               other.BuiltIn == BuiltIn && other.Saved == Saved && other.Title == Title;
    }
}