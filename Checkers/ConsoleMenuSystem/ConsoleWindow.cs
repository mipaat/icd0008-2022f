using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleMenuSystem;

public record ConsoleLine(string Content = "", ConsoleColor? BackgroundColor = null, EAlignment Align = EAlignment.Left)
{
    public readonly string Content = Content;
    public readonly EAlignment Align = Align;
    public readonly ConsoleColor? BackgroundColor = BackgroundColor;
}

public class ConsoleWindow
{
    public string Title;
    public readonly int Width;
    public readonly int Height;
    private ConsoleColor _highlightColor = ConsoleColor.White;
    private bool _cursorVisible;

    public bool CursorVisible
    {
        get => _cursorVisible;
        set
        {
            Console.CursorVisible = value;
            _cursorVisible = value;
        }
    }

    private List<ConsoleLine>? _renderQueue;

    public ConsoleWindow(string title = "ConsoleWindow", int width = 72, int height = 12)
    {
        Title = CheckStringValid(title);
        Width = width;
        Height = height;
    }

    private void SyncCursorVisibility()
    {
        Console.CursorVisible = CursorVisible;
    }

    private void DiscardRenderQueue()
    {
        _renderQueue = null;
    }

    public void Render()
    {
        SyncCursorVisibility();
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

    private static string CheckStringValid(string content)
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

        _renderQueue.Add(new ConsoleLine(content, highlight ? _highlightColor : null));
    }

    public void AddLine(char pattern, int? amount = null)
    {
        var conformedAmount = (amount is not null) ? Math.Min((int)amount, Width) : Width;
        AddLine(new string(pattern, conformedAmount));
    }

    public int LinesLeft()
    {
        if (_renderQueue is not null)
        {
            return Height - _renderQueue.Count;
        }
        return Height;
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

    private static bool? ConsoleCursorVisible()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Console.CursorVisible;
        return null;
    }

    public string? PromptTextInput(string prompt, string? rePrompt = null)
    {
        DiscardRenderQueue();

        if (rePrompt is not null && rePrompt.Trim().Length > 0) AddLine(CheckStringValid(rePrompt));
        AddLine(CheckStringValid(prompt));

        var inputCursorPosition = Console.GetCursorPosition();
        inputCursorPosition.Top += 2;
        Render();
        var finalCursorPosition = Console.GetCursorPosition();
        var previousCursorVisible = CursorVisible;

        Console.SetCursorPosition(inputCursorPosition.Left, inputCursorPosition.Top);
        CursorVisible = true;

        var result = Console.ReadLine();

        CursorVisible = previousCursorVisible;
        Console.SetCursorPosition(finalCursorPosition.Left, finalCursorPosition.Top);

        return result;
    }

    public void MessageBox(string message)
    {
        DiscardRenderQueue();
        var messageLines = message.Split("\n");
        if (messageLines.Length > LinesLeft() - 1)
            throw new ArgumentOutOfRangeException(
                $"Message '{message}' contains too many lines ({messageLines.Length} > {LinesLeft() - 1})!");

        foreach (var line in messageLines)
        {
            AddLine(line);
        }

        AddLine("Press any key to continue");
        Render();
        Console.ReadKey();
    }
}