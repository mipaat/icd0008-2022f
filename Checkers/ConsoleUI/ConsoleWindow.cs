using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleUI;

public class ConsoleLine
{
    private readonly string _content;
    public readonly EAlignment Align;
    public readonly ConsoleColor? _backgroundColor;
    public ConsoleColor BackgroundColor => _backgroundColor ?? Console.BackgroundColor;
    private readonly bool _truncationPreferRight;
    private readonly int _patternRepetitionAmount;

    public ConsoleLine(string content = "", ConsoleColor? backgroundColor = null, EAlignment align = EAlignment.Left,
        bool truncationPreferRight = false, int patternRepetitionAmount = 1)
    {
        _content = content;
        _backgroundColor = backgroundColor;
        Align = align;
        _truncationPreferRight = truncationPreferRight;
        _patternRepetitionAmount = patternRepetitionAmount;
    }

    public string Content()
    {
        {
            if (_patternRepetitionAmount > 1)
            {
                var repeatingStringBuilder = new StringBuilder(_content.Length * _patternRepetitionAmount).Insert(0,
                    _content,
                    _patternRepetitionAmount); //TODO: Optimize

                return repeatingStringBuilder.ToString()[..Console.WindowWidth];
            }

            return _content.Length > Console.WindowWidth
                ? _truncationPreferRight
                    ? "..." + _content[(_content.Length - Console.WindowWidth + 3)..]
                    : _content[..(Console.WindowWidth - 3)] + "..."
                : _content;
        }
    }
}

public class ConsoleWindow
{
    public string Title;
    public readonly Encoding OutputEncoding = Encoding.UTF8;

    private List<ConsoleLine> _renderQueue;

    private const ConsoleColor HighlightColor = ConsoleColor.White;
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

    private int _cursorPosition;

    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            _cursorPosition = value;
            SyncCursorPosition();
        }
    }

    public ConsoleWindow(string title = "ConsoleWindow", int width = 72, int height = 12)
    {
        Title = CheckStringValid(title);
        CursorVisible = false;
        _renderQueue = new List<ConsoleLine>();
    }

    private void SyncCursorPosition()
    {
        Console.SetCursorPosition(0, _cursorPosition + 1);
    }

    private void SyncCursorVisibility()
    {
        Console.CursorVisible = CursorVisible;
    }

    public void ResetCursorPosition()
    {
        CursorPosition = 0;
    }

    public void ClearRenderQueue()
    {
        _renderQueue.Clear();
    }

    public void Render(bool resetCursorPosition = true)
    {
        var previousOutputEncoding = Console.OutputEncoding;
        Console.OutputEncoding = OutputEncoding;

        SyncCursorVisibility();
        SyncCursorPosition();

        WriteLine(new ConsoleLine(Title));
        for (var i = 0; i < Math.Min(_renderQueue.Count, Console.WindowHeight - 2); i++)
        {
            WriteLine(_renderQueue[i]);
        }

        var afterContentCursorPosition = CursorPosition;

        FillRemainingSpace();

        CursorPosition = afterContentCursorPosition;

        if (resetCursorPosition) ResetCursorPosition();
        ClearRenderQueue();

        Console.OutputEncoding = previousOutputEncoding;
    }

    private void FillRemainingSpace()
    {
        for (var i = Console.CursorTop; i < Console.WindowHeight - Console.WindowTop - 1; i++)
        {
            WriteLine();
        }
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

    public void AddLine(string content = "", bool highlight = false, bool truncationPreferRight = false)
    {
        CheckStringValid(content);

        _renderQueue.Add(new ConsoleLine(content, highlight ? HighlightColor : null,
            truncationPreferRight: truncationPreferRight));
    }

    public void AddLine(char pattern, int? amount = null)
    {
        if (amount != null)
        {
            AddLine(new string(pattern, amount.Value));
        }
        else
        {
        }
    }

    private void WriteLine(ConsoleLine? line = null)
    {
        var content = line?.Content() ?? "";

        var result = new StringBuilder(content);

        var totalSpaceToAdd = Console.WindowWidth - content.Length;

        switch (line?.Align)
        {
            default:
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

        var previousBackgroundColor = Console.BackgroundColor;
        if (line is not null) Console.BackgroundColor = line.BackgroundColor;
        Console.WriteLine(result.ToString()); //TODO: Highlight only actual line content?
        CursorPosition++;
        Console.BackgroundColor = previousBackgroundColor;
    }

    public string? PopupPromptTextInput(string prompt, string? rePrompt = null)
    {
        ClearRenderQueue();

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

    public string? RenderAndAwaitTextInput(string prompt, bool keepRenderQueue = false)
    {
        var previousRenderQueue = new List<ConsoleLine>(_renderQueue);

        AddLine(prompt);
        Render(false);

        var previousCursorVisible = CursorVisible;
        CursorVisible = true;
        var result = Console.ReadLine();
        CursorVisible = previousCursorVisible;
        ResetCursorPosition();

        if (keepRenderQueue) _renderQueue.AddRange(previousRenderQueue);

        return result;
    }

    public void MessageBox(params string[] messageLines)
    {
        ClearRenderQueue();

        var processedMessageLines = new List<string>();

        foreach (var line in messageLines)
        {
            processedMessageLines.AddRange(line.Split("\n"));
        }

        foreach (var line in processedMessageLines)
        {
            AddLine(line);
        }

        AddLine("Press any key to continue");
        Render();
        Console.ReadKey();
    }

    public void Close()
    {
        ClearRenderQueue();
        Render();
        ResetCursorPosition();
        Console.WriteLine($"{Title} closed");
    }
}