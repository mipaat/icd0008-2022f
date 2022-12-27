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