using System.Text;
using System;

namespace ConsoleUI;

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

    public ConsoleWindow(string title = "ConsoleWindow")
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

    private void WriteLineNoCursorAdvance(string? line = null, EAlignment? align = null,
        ConsoleColor? backgroundColor = null)
    {
        var content = CheckStringValid(line ?? "");
        var shortenContent = content.Length > Console.WindowWidth;
        if (shortenContent)
        {
            int newWidth;
            switch (align)
            {
                default:
                    newWidth = Console.WindowWidth - 3;
                    content = content[..newWidth] + "...";
                    break;
                case EAlignment.Center:
                    var middle = content.Length / 2;
                    newWidth = Console.WindowWidth - 6;
                    content = string.Concat("...", content.AsSpan(middle - newWidth / 2, newWidth), "...");
                    break;
                case EAlignment.Right:
                    newWidth = Console.WindowWidth - 3;
                    content = string.Concat("...", content.AsSpan(content.Length - newWidth, newWidth));
                    break;
            }
        }

        var result = new StringBuilder(content);

        var totalSpaceToAdd = Console.WindowWidth - content.Length;

        var leftAddedSpace = 0;

        switch (align)
        {
            default:
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

        var initialCursorLeft = Console.CursorLeft;
        var initialCursorTop = Console.CursorTop;

        Console.Write(resultString[..leftAddedSpace]);

        var previousBackgroundColor = Console.BackgroundColor;
        Console.BackgroundColor = backgroundColor ?? previousBackgroundColor;

        Console.Write(resultString[leftAddedSpace..(leftAddedSpace + content.Length)]);

        Console.BackgroundColor = previousBackgroundColor;

        Console.Write(resultString[(leftAddedSpace + content.Length)..]);
        
        // For some reason Console.Write() adds a newline SOMETIMES
        // The following lines undo that by forcing the console cursor to the expected position
        Console.CursorLeft = Math.Min(initialCursorLeft + leftAddedSpace + content.Length, Console.WindowWidth - 1);
        Console.CursorTop = initialCursorTop;
    }

    private void WriteLine(string? line = null, EAlignment? align = null, ConsoleColor? backgroundColor = null)
    {
        WriteLineNoCursorAdvance(line, align, backgroundColor);

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

    public bool PopupPromptBoolInput(string prompt)
    {
        bool IsValidBooleanString(string input, out bool result)
        {
            switch (input.Trim().ToLower())
            {
                case "y":
                    result = true;
                    break;
                case "n":
                    result = false;
                    break;
                default:
                    result = default!;
                    return false;
            }

            return true;
        }

        string? input = null;
        while (true)
        {
            var rePrompt = input == null ? null : "Input must be 'y' for yes or 'n' for no!";
            input = PopupPromptTextInput(prompt + " (y/n)", rePrompt);

            if (IsValidBooleanString(input, out var result)) return result;
        }
    }

    public delegate bool ValidityFunc<in T>(T input, out string? rePrompt);

    public int PopupPromptIntInput(string prompt, ValidityFunc<int>? validityFunc = null)
    {
        string? rePrompt = null;
        while (true)
        {
            var textInput = PopupPromptTextInput(prompt, rePrompt);
            if (int.TryParse(textInput, out var input))
            {
                if (validityFunc != null && !validityFunc(input, out rePrompt))
                {
                    continue;
                }

                return input;
            }

            rePrompt = $"Couldn't get integer from '{textInput}'!";
        }
    }

    public ConsoleInput AwaitInput(params ConsoleKeyInfoBasic[] customExitConditions)
    {
        var result = new ConsoleInput();
        var shouldBreak = false;
        var exitConditions = new List<ConsoleKeyInfoBasic>
        {
            new(ConsoleKey.Q, false, false, true),
            new(ConsoleKey.Escape)
        };
        exitConditions.AddRange(customExitConditions);

        var initialCursorLeft = Console.CursorLeft;

        do
        {
            var keyInfo = Console.ReadKey(true);
            if (exitConditions.Exists(cki => cki.Equals(keyInfo)))
            {
                result.Clear();
                result.Add(keyInfo);
                shouldBreak = true;
            }
            else
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        shouldBreak = true;
                        break;
                    case ConsoleKey.Backspace:
                        result.RemoveLeft();
                        break;
                    case ConsoleKey.Delete:
                        result.RemoveRight();
                        break;
                    case ConsoleKey.LeftArrow:
                        result.MoveLeft();
                        break;
                    case ConsoleKey.RightArrow:
                        result.MoveRight();
                        break;
                    case ConsoleKey.UpArrow:
                        break;
                    case ConsoleKey.DownArrow:
                        break;
                    case ConsoleKey.End:
                        result.MoveRight(result.Size - result.Position);
                        break;
                    case ConsoleKey.Home:
                        result.MoveLeft(result.Position);
                        break;
                    default:
                        result.Add(keyInfo);
                        Console.Write(keyInfo.KeyChar);
                        break;
                }
            }

            var previousCursorVisible = CursorVisible;
            CursorVisible = false;
            Console.CursorLeft = initialCursorLeft;

            WriteLineNoCursorAdvance(result.Text);

            CursorVisible = previousCursorVisible;
            Console.CursorLeft = initialCursorLeft + result.Position;
        } while (!shouldBreak);

        Console.CursorLeft = initialCursorLeft;
        return result;
    }

    public ConsoleInput RenderAndAwaitTextInput(string prompt, bool keepRenderQueue = false)
    {
        var previousRenderQueue = new List<ConsoleLine>(_renderQueue);

        AddLine(prompt);
        Render(false);

        var previousCursorVisible = CursorVisible;
        CursorVisible = true;
        var result = AwaitInput();
        CursorVisible = previousCursorVisible;
        ResetCursorPosition();

        if (keepRenderQueue) _renderQueue.AddRange(previousRenderQueue);

        return result;
    }

    public void PopUpMessageBox(params string[] messageLines)
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