using System.Text;
using System.Text.RegularExpressions;
using ConsoleUI;
using DAL;
using Domain;
using GameBrain;

namespace ConsoleClient;

public class ConsoleGame
{
    private readonly AbstractCheckersBrain _checkersBrain;
    private readonly ConsoleWindow _consoleWindow;
    private readonly IRepositoryContext _repositoryContext;

    public ConsoleGame(ConsoleWindow consoleWindow, AbstractCheckersBrain checkersBrain,
        IRepositoryContext repositoryContext)
    {
        _checkersBrain = checkersBrain;
        _consoleWindow = consoleWindow;
        _repositoryContext = repositoryContext;
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
            var coordsString = EncodeLetterCoords(i);
            var conformedCoordsString = coordsString.Length > 3 ? "N/A" : coordsString.PadLeft(2).PadRight(3);
            result.Append(conformedCoordsString);
            result.Append(" ");
        }

        return result.ToString();
    }

    private static string EncodeLetterCoords(int letterCoords)
    {
        var charId = letterCoords % 26;
        var charMultiplier = letterCoords / 26 + 1;
        var letter = (char)('A' + charId);
        return new string(letter, charMultiplier);
    }

    private static int DecodeLetterCoords(string letterCoords)
    {
        const int baseOffset = 'A';

        if (letterCoords.Length == 0) throw new ArgumentOutOfRangeException();

        return (letterCoords.Length - 1) * 26 + (letterCoords[0] - baseOffset);
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
        var breakControl = true;
        var rx = new Regex(@"([A-Za-z]+)(\d+)([A-Za-z]+)(\d+)");
        while (breakControl)
        {
            AddBoardToRenderQueue();
            var input = _consoleWindow
                .RenderAndAwaitTextInput(
                    "Q to quit! Coordinate pairs (e.g A4D7) to move (currently no game logic/rules implemented)!",
                    keepRenderQueue: true)?.ToLower() ?? "";
            switch (input)
            {
                case "q":
                    breakControl = false;
                    break;
                default:
                    var matches = rx.Matches(input);
                    if (matches.Count == 1)
                    {
                        try
                        {
                            var groups = matches[0].Groups;
                            var x1 = DecodeLetterCoords(groups[1].Value.ToUpper());
                            var y1 = _checkersBrain.Height - int.Parse(groups[2].Value);
                            var x2 = DecodeLetterCoords(groups[3].Value.ToUpper());
                            var y2 = _checkersBrain.Height - int.Parse(groups[4].Value);
                            if (_checkersBrain.IsMoveValid(x1, y1, x2, y2))
                            {
                                _checkersBrain.Move(x1, y1, x2, y2);
                            }
                        }
                        catch (Exception e)
                        {
                            _consoleWindow.PopupPromptTextInput("Input caused the following error: " +
                                                                e.ToString().Split("\n")[0]);
                        }
                    }

                    break;
            }

            _consoleWindow.ClearRenderQueue();
        }

        var saveGameInput = _consoleWindow.PopupPromptBoolInput("Save game?");

        if (saveGameInput) _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.GetSaveGameState());
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