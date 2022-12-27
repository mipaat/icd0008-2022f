using System.Text;

namespace ConsoleUI;

public record ConsoleInput
{
    private int _position;
    public int Position => _position;
    public int Size => KeyInfos.Count;

    public string Text {
        get
        {
            var result = new StringBuilder();
            foreach (var keyInfo in KeyInfos)
            {
                result.Append(keyInfo.KeyChar);
            }

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
            if (_position >= KeyInfos.Count)
            {
                KeyInfos.Add(consoleKeyInfo);
            }
            else
            {
                KeyInfos.Insert(_position, consoleKeyInfo);
            }

            _position++;
        }
    }

    public void RemoveRight(int amount = 1)
    {
        if (amount < 0) throw new ArgumentException($"{nameof(amount)} must not be negative!");
        KeyInfos.RemoveRange(_position, Math.Min(amount, KeyInfos.Count - _position));
    }
    
    public void RemoveLeft(int amount = 1)
    {
        if (amount < 0) throw new ArgumentException($"{nameof(amount)} must not be negative!");
        var startIndex = Math.Max(_position - amount, 0);
        var actualAmount = _position - startIndex;
        KeyInfos.RemoveRange(startIndex, actualAmount);
        _position -= actualAmount;
    }

    public void Clear()
    {
        KeyInfos.Clear();
        _position = 0;
    }

    public void MoveRight(int amount = 1)
    {
        MoveLeft(-amount);
    }

    public void MoveLeft(int amount = 1)
    {
        _position = Math.Max(0, _position - amount);
    }
}