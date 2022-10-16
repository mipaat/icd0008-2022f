using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleUI;

public class ConsoleLine
{
    private readonly string _content;
    public readonly EAlignment Align;
    public readonly ConsoleColor? BackgroundColor;
    private readonly bool _truncationPreferRight;
    private readonly int? _patternRepetitionAmount;

    private int PatternRepetitionAmount => _patternRepetitionAmount ?? Console.WindowWidth / _content.Length + 1;

    public ConsoleLine(string content = "", ConsoleColor? backgroundColor = null, EAlignment align = EAlignment.Left,
        bool truncationPreferRight = false, int? patternRepetitionAmount = 1)
    {
        _content = content;
        BackgroundColor = backgroundColor;
        Align = align;
        _truncationPreferRight = truncationPreferRight;
        _patternRepetitionAmount = patternRepetitionAmount;
    }

    public string Content()
    {
        {
            if (PatternRepetitionAmount > 1)
            {
                var repeatingStringBuilder = new StringBuilder(_content.Length * PatternRepetitionAmount).Insert(0,
                    _content,
                    PatternRepetitionAmount); //TODO: Optimize

                var result = repeatingStringBuilder.ToString();
                return result[..Math.Min(Console.WindowWidth, result.Length)];
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

    public List<ConsoleLine> RenderQueue
    {
        get
        {
            var result = new List<ConsoleLine>(_renderQueue);
            var titleHorizontalBorder = new ConsoleLine(new string('-', Title.Length + 4));
            result.Insert(0, titleHorizontalBorder);
            result.Insert(1, new ConsoleLine("| " + Title + " |"));
            result.Insert(2, titleHorizontalBorder);
            result.Insert(0, new ConsoleLine());
            return result;
        }
    }

    public void Render(bool resetCursorPosition = true)
    {
        var previousOutputEncoding = Console.OutputEncoding;
        Console.OutputEncoding = OutputEncoding;

        SyncCursorVisibility();
        SyncCursorPosition();

        var windowIsCutOff = Console.WindowHeight - 1 <= RenderQueue.Count;

        var maxRenderedLines = windowIsCutOff ? Console.WindowHeight - 1 : RenderQueue.Count;

        for (var i = 0; i < maxRenderedLines; i++)
        {
            WriteLine(RenderQueue[i]);
        }

        if (windowIsCutOff) WriteLine(new ConsoleLine("▲▼", patternRepetitionAmount: null));

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

    public void AddLine(string content = "", ConsoleColor? backgroundColor = null, bool truncationPreferRight = false)
    {
        CheckStringValid(content);

        _renderQueue.Add(new ConsoleLine(content, backgroundColor,
            truncationPreferRight: truncationPreferRight));
    }

    public void AddLinePattern(string pattern, int? amount = null)
    {
        CheckStringValid(pattern);

        _renderQueue.Add(new ConsoleLine(pattern, patternRepetitionAmount: amount));
    }

    private void WriteLine(ConsoleLine? line)
    {
        WriteLine(line?.Content(), line?.Align, line?.BackgroundColor);
    }

    private void WriteLine(string? line = null, EAlignment? align = null, ConsoleColor? backgroundColor = null)
    {
        var content = line ?? "";

        var result = new StringBuilder(content);

        var totalSpaceToAdd = Console.WindowWidth - content.Length;

        var leftAddedSpace = 0;

        switch (align)
        {
            default:
            case EAlignment.Left:
                result.Append(' ', totalSpaceToAdd);
                break;
            case EAlignment.Center:
                var leftSpace = totalSpaceToAdd / 2;
                var rightSpace = totalSpaceToAdd - leftSpace;
                result.Insert(0, " ", leftSpace).Append(' ', rightSpace);
                leftAddedSpace += leftSpace;
                break;
            case EAlignment.Right:
                result.Insert(0, " ", totalSpaceToAdd);
                leftAddedSpace += totalSpaceToAdd;
                break;
        }

        var resultString = result.ToString();
        Console.Write(resultString[..leftAddedSpace]);

        var previousBackgroundColor = Console.BackgroundColor;
        Console.BackgroundColor = backgroundColor ?? previousBackgroundColor;

        Console.Write(resultString[leftAddedSpace..(leftAddedSpace + content.Length)]);

        Console.BackgroundColor = previousBackgroundColor;

        Console.Write(resultString[(leftAddedSpace + content.Length)..]);

        CursorPosition++;
    }

    public string PopupPromptTextInput(string prompt, string? rePrompt = null)
    {
        ClearRenderQueue();

        if (rePrompt is not null && rePrompt.Trim().Length > 0) AddLine(CheckStringValid(rePrompt));
        AddLine(CheckStringValid(prompt));

        var inputCursorPosition = Console.GetCursorPosition();
        inputCursorPosition.Top += RenderQueue.Count;
        Render();
        var finalCursorPosition = Console.GetCursorPosition();
        var previousCursorVisible = CursorVisible;

        Console.SetCursorPosition(inputCursorPosition.Left, inputCursorPosition.Top);
        CursorVisible = true;

        var result = Console.ReadLine() ?? "";

        CursorVisible = previousCursorVisible;
        Console.SetCursorPosition(finalCursorPosition.Left, finalCursorPosition.Top);

        return result;
    }

    public bool PopupPromptBoolInput(string prompt, string? rePrompt = null)
    {
        var input = PopupPromptTextInput(prompt + " (y/n)", rePrompt);
        return input.Trim().ToLower() switch
        {
            "y" => true,
            "n" => false,
            _ => throw new FormatException($"Input '{input}' is not a valid boolean string!")
        };
    }

    public int PopupPromptIntInput(string prompt, string? rePrompt = null)
    {
        return int.Parse(PopupPromptTextInput(prompt, rePrompt));
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
        ResetCursorPosition();
        FillRemainingSpace();
        ResetCursorPosition();
        WriteLine($"{Title} closed");
    }
}