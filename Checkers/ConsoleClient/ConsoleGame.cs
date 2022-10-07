using System.Text;
using ConsoleUI;
using Game;

namespace ConsoleClient;

public class ConsoleGame
{
    private readonly AbstractGameBrain _gameBrain;
    private readonly ConsoleWindow _consoleWindow;

    private const string WhitePieceDisplay = "W";
    private const string BlackPieceDisplay = "B";

    private string _player1Id;
    private string? _player2Id;

    public ConsoleGame(ConsoleWindow consoleWindow, AbstractGameBrain gameBrain)
    {
        _gameBrain = gameBrain;
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

    private void AddBoardToRenderQueue()
    {
        for (var y = 0; y < _gameBrain.Height; y++)
        {
            _consoleWindow.AddLine(BoundaryLine(_gameBrain.Width));
            var line = new StringBuilder("|");
            for (var x = 0; x < _gameBrain.Width; x++)
            {
                line.Append(GetPieceDisplay(_gameBrain[x, y]));
                line.Append('|');
            }

            _consoleWindow.AddLine(line.ToString());
        }

        _consoleWindow.AddLine(BoundaryLine(_gameBrain.Width));
    }

    public void Run()
    {
        AddBoardToRenderQueue();
        _consoleWindow.RenderAndAwaitTextInput("Press any key to quit");
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