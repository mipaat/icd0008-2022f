using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleUI;

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
    public readonly Encoding OutputEncoding = Encoding.UTF8;

    private List<ConsoleLine> _renderQueue;

    private ConsoleColor _highlightColor = ConsoleColor.White;
    private bool _cursorVisible;
    public readonly (int Left, int Top) InitialCursorPosition;


    public bool CursorVisible
    {
        get => _cursorVisible;
        set
        {
            Console.CursorVisible = value;
            _cursorVisible = value;
        }
    }

    private (int Left, int Top) _cursorPosition;

    public ConsoleWindow(string title = "ConsoleWindow", int width = 72, int height = 12)
    {
        Title = CheckStringValid(title);
        Width = width;
        Height = height;
        InitialCursorPosition = Console.GetCursorPosition();
        _renderQueue = new List<ConsoleLine>();
    }

    public (int Left, int Top) GetCursorPosition()
    {
        return (_cursorPosition.Left, _cursorPosition.Top);
    }

    public void SetCursorPosition(int left, int top)
    {
        _cursorPosition = (left, top);
        SyncCursorPosition();
    }

    private void SyncCursorPosition()
    {
        Console.SetCursorPosition(InitialCursorPosition.Left + _cursorPosition.Left, InitialCursorPosition.Top + _cursorPosition.Top);
    }
    
    private void SyncCursorVisibility()
    {
        Console.CursorVisible = CursorVisible;
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
        for (var i = 0; i < Math.Min(Height, Console.WindowHeight - Console.WindowTop - 2); i++)
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

        if (resetCursorPosition) ResetCursorPosition();
        ClearRenderQueue();

        Console.OutputEncoding = previousOutputEncoding;
    }

    public void ResetCursorPosition()
    {
        SetCursorPosition(0, 0);
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
        return Height - _renderQueue.Count;
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

    public void MessageBox(string message)
    {
        ClearRenderQueue();
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

    public void Close()
    {
        ClearRenderQueue();
        Render();
        ResetCursorPosition();
    }
}