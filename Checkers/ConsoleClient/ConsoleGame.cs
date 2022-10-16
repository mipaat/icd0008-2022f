using System.Text;
using ConsoleUI;
using Domain;
using GameBrain;

namespace ConsoleClient;

public class ConsoleGame
{
    private readonly AbstractCheckersBrain _checkersBrain;
    private readonly ConsoleWindow _consoleWindow;

    private string _player1Id;
    private string? _player2Id;

    public ConsoleGame(ConsoleWindow consoleWindow, AbstractCheckersBrain checkersBrain)
    {
        _checkersBrain = checkersBrain;
        _consoleWindow = consoleWindow;
        _player1Id = "1";
        _player2Id = null;
    }

    private static string GetPieceDisplay(GamePiece? gamePiece)
    {
        var pieceIcon = (gamePiece?.Player, gamePiece?.IsCrowned) switch
        {
            (EPlayerColor.Black, false) => "⬤",
            (EPlayerColor.White, false) => "◯",
            (EPlayerColor.Black, true) => "♛",
            (EPlayerColor.White, true) => "♕",
            _ => " "
        };
        return " " + pieceIcon + " ";
    }

    private static string LettersLine(int width)
    {
        var result = new StringBuilder(" ");
        for (var i = 0; i < width; i++)
        {
            var charId = i % 26;
            var charMultiplier = i / 26 + 1;
            var letter = (char)('A' + charId);
            result.Append(charMultiplier > 3 ? "N/A" : (new string(letter, charMultiplier)).PadLeft(2).PadRight(3));

            result.Append(" ");
        }

        return result.ToString();
    }

    private void AddBoardToRenderQueue()
    {
        _consoleWindow.AddLine(LettersLine(_checkersBrain.Width));
        for (var y = 0; y < _checkersBrain.Height; y++)
        {
            _consoleWindow.AddLine(BoundaryLine(_checkersBrain.Width));
            var line = new StringBuilder("|");
            for (var x = 0; x < _checkersBrain.Width; x++)
            {
                line.Append(GetPieceDisplay(_checkersBrain[x, y]));
                line.Append('|');
            }

            line.Append($" {_checkersBrain.Height - y}");

            _consoleWindow.AddLine(line.ToString());
        }

        _consoleWindow.AddLine(BoundaryLine(_checkersBrain.Width));
    }

    public void Run()
    {
        string? input = null;

        while ((input ?? "").ToLower() != "q")
        {
            AddBoardToRenderQueue();
            input = _consoleWindow.RenderAndAwaitTextInput("Type Q (and hit Enter) to quit!", keepRenderQueue: true);
            _consoleWindow.ClearRenderQueue();
        }
    }

    private static string BoundaryLine(int cells)
    {
        var result = new StringBuilder();
        for (var i = 0; i < cells; i++)
        {
            result.Append("+---");
        }

        result.Append("+");
        return result.ToString();
    }
}