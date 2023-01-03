using System.Text;

namespace ConsoleUI;

public record ConsoleInput
{
    public int Position { get; private set; }

    public int Size => KeyInfos.Count;

    public string Text
    {
        get
        {
            var result = new StringBuilder();
            foreach (var keyInfo in KeyInfos) result.Append(keyInfo.KeyChar);

            return result.ToString();
        }
    }

    private List<ConsoleKeyInfo> KeyInfos { get; } = new();
    public ConsoleKeyInfo? KeyInfo => IsKeyPress ? KeyInfos[^1] : null;
    public bool IsKeyPress => KeyInfos.Count == 1;

    public void Add(params ConsoleKeyInfo[] keyInfos)
    {
        foreach (var consoleKeyInfo in keyInfos)
        {
            if (Position >= KeyInfos.Count)
                KeyInfos.Add(consoleKeyInfo);
            else
                KeyInfos.Insert(Position, consoleKeyInfo);

            Position++;
        }
    }

    public void RemoveRight(int amount = 1)
    {
        if (amount < 0) throw new ArgumentException($"{nameof(amount)} must not be negative!");
        KeyInfos.RemoveRange(Position, Math.Min(amount, KeyInfos.Count - Position));
    }

    public void RemoveLeft(int amount = 1)
    {
        if (amount < 0) throw new ArgumentException($"{nameof(amount)} must not be negative!");
        var startIndex = Math.Max(Position - amount, 0);
        var actualAmount = Position - startIndex;
        KeyInfos.RemoveRange(startIndex, actualAmount);
        Position -= actualAmount;
    }

    public void Clear()
    {
        KeyInfos.Clear();
        Position = 0;
    }

    public void MoveRight(int amount = 1)
    {
        MoveLeft(-amount);
    }

    public void MoveLeft(int amount = 1)
    {
        Position = Math.Max(0, Position - amount);
    }
}