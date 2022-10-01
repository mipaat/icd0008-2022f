using System.Text;

namespace ConsoleMenuSystem;

public record ConsoleLine(string Content = "", ConsoleColor? BackgroundColor = null, EAlignment Align = EAlignment.Left)
{
    public readonly string Content = Content;
    public readonly EAlignment Align = Align;
    public readonly ConsoleColor? BackgroundColor = BackgroundColor;

    public ConsoleLine(string content, bool highlight = false, EAlignment align = EAlignment.Left) : this(content,
        highlight ? ConsoleColor.White : null, align)
    {
    }
}

public class ConsoleWindow
{
    public string Title;
    public readonly int Width;
    public readonly int Height;
    private ConsoleColor _highlightColor = ConsoleColor.White;

    private List<ConsoleLine>? _renderQueue;

    public ConsoleWindow(string title = "ConsoleWindow", int width = 72, int height = 12)
    {
        Title = CheckStringValid(title);
        Width = width;
        Height = height;
    }

    public void DiscardRenderQueue()
    {
        _renderQueue = null;
    }

    public void Render()
    {
        var initialCursorTop = Console.CursorTop;

        Console.WriteLine(Title);
        for (var i = 0; i < Height; i++)
        {
            var previousBackgroundColor = Console.BackgroundColor;
            if (_renderQueue is not null && i < _renderQueue.Count)
            {
                WriteLine(_renderQueue[i]);
            }
            else
            {
                WriteLine();
            }

            Console.BackgroundColor = previousBackgroundColor;
        }

        Console.SetCursorPosition(0, initialCursorTop);
        DiscardRenderQueue();
    }

    public static string CheckStringValid(string content)
    {
        if (content.Contains('\n') || content.Contains('\r'))
        {
            throw new ArgumentException($"Content can't contain characters that affect line structure!",
                nameof(content));
        }

        return content;
    }

    private string ConformLength(string text, bool preferRight = false)
    {
        if (preferRight)
        {
            return text.Length > Width ? "..." + text[(text.Length - Width + 3)..] : text;
        }

        return text.Length > Width ? text[..(Width - 3)] + "..." : text;
    }

    public void AddLine(string content = "", bool highlight = false, bool conformLength = false,
        bool preferRight = false)
    {
        if (content.Length > Width)
        {
            if (conformLength)
            {
                content = ConformLength(content, preferRight);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(content),
                    $"Content length {content.Length} exceeds max line width {Width}!");
            }
        }

        CheckStringValid(content);

        _renderQueue ??= new List<ConsoleLine>();

        if (_renderQueue.Count >= Height)
        {
            throw new RenderQueueFullException(content);
        }

        _renderQueue.Add(new ConsoleLine(content, highlight));
    }

    public void AddLine(char pattern, int? amount = null)
    {
        var conformedAmount = (amount is not null) ? Math.Min((int)amount, Width) : Width;
        AddLine(new string(pattern, conformedAmount));
    }

    public int LinesLeft()
    {
        return Height - _renderQueue?.Count ?? 0;
    }

    private void WriteLine(ConsoleLine? line = null)
    {
        var content = line?.Content ?? "";
        var previousBackgroundColor = Console.BackgroundColor;
        Console.BackgroundColor = line?.BackgroundColor ?? previousBackgroundColor;

        var result = new StringBuilder(content);
        var totalSpaceToAdd = Width - content.Length;
        switch (line?.Align)
        {
            default:
            case null:
            case EAlignment.Left:
                result.Append(' ', totalSpaceToAdd);
                break;
            case EAlignment.Center:
                var leftSpace = totalSpaceToAdd / 2;
                var rightSpace = totalSpaceToAdd - leftSpace;
                result.Insert(0, " ", leftSpace).Append(' ', rightSpace);
                break;
            case EAlignment.Right:
                result.Insert(0, " ", totalSpaceToAdd);
                break;
        }

        Console.WriteLine(result.ToString());
    }
}