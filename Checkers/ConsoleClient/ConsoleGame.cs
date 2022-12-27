using System.Text;
using System.Text.RegularExpressions;
using ConsoleUI;
using DAL;
using Domain;
using GameBrain;

namespace ConsoleClient;

public class ConsoleGame
{
    private CheckersBrain _checkersBrain;
    private readonly ConsoleWindow _consoleWindow;
    private readonly IRepositoryContext _repositoryContext;
    private readonly EPlayerColor? _playMode;

    public ConsoleGame(ConsoleWindow consoleWindow, CheckersBrain checkersBrain,
        IRepositoryContext repositoryContext, EPlayerColor? playMode = null)
    {
        _checkersBrain = checkersBrain;
        _consoleWindow = consoleWindow;
        _repositoryContext = repositoryContext;
        _playMode = playMode;
    }

    private void Refresh()
    {
        var checkersGame = _repositoryContext.CheckersGameRepository.GetById(_checkersBrain.CheckersGame.Id);
        if (checkersGame == null) throw new IllegalStateException("CheckersGame was null after refresh!");
        _checkersBrain = new CheckersBrain(checkersGame);
    }

    private static string GetPieceDisplay(GamePiece? gamePiece)
    {
        var pieceIcon = (gamePiece?.Player, gamePiece?.IsCrowned) switch
        {
            (EPlayerColor.White, false) => "⬤",
            (EPlayerColor.Black, false) => "◯",
            (EPlayerColor.White, true) => "♛",
            (EPlayerColor.Black, true) => "♕",
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
            result.Append(' ');
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
        _consoleWindow.AddLine($"{_checkersBrain.CurrentTurnPlayerColor} player's turn");
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

    private void MakePlayerMove(ConsoleInput input)
    {
        var rx = new Regex(@"([A-Za-z]+)(\d+)([A-Za-z]+)(\d+)");
        var matches = rx.Matches(input.Text);
        if (matches.Count == 1)
        {
            try
            {
                var groups = matches[0].Groups;
                var x1 = DecodeLetterCoords(groups[1].Value.ToUpper());
                var y1 = _checkersBrain.Height - int.Parse(groups[2].Value);
                var x2 = DecodeLetterCoords(groups[3].Value.ToUpper());
                var y2 = _checkersBrain.Height - int.Parse(groups[4].Value);
                if (_checkersBrain.IsMoveValid(_checkersBrain.CurrentTurnPlayerColor, x1, y1, x2,
                        y2))
                {
                    _checkersBrain.Move(x1, y1, x2, y2);
                    _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
                }
                else
                {
                    _consoleWindow.PopUpMessageBox("Invalid move!");
                }
            }
            catch (Exception e)
            {
                _consoleWindow.PopUpMessageBox("Input caused the following error: " +
                                               e.ToString().Split("\n")[0]);
            }
        }
    }

    private void RunAiTurn()
    {
        _consoleWindow.Render();
        var timer = new System.Timers.Timer(1000);
        var timerElapsed = false;
        timer.Elapsed += (_, _) => timerElapsed = true;
        _checkersBrain.MoveAi();
        while (!timerElapsed)
        {
        }

        _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
    }

    private bool HandlePlayerInput()
    {
        var prompt = "Ctrl+Q to quit! Coordinate pairs (e.g A4D7) to move!";
        if (_checkersBrain.EndTurnAllowed) prompt += " E to end turn!";

        var input = _consoleWindow
            .RenderAndAwaitTextInput(
                prompt,
                keepRenderQueue: true);
        var inputTextNormalized = input.Text.ToLower();

        if (input.IsKeyPress)
        {
            return input.KeyInfo is { Key: ConsoleKey.Q, Modifiers: ConsoleModifiers.Control };
        }

        if (inputTextNormalized == "e" && _checkersBrain.EndTurnAllowed)
        {
            _checkersBrain.EndTurn();
            _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
            return false;
        }

        MakePlayerMove(input);
        return false;
    }
    
    public void Run()
    {
        var shouldQuit = false;
        while (!shouldQuit)
        {
            Refresh();
            AddBoardToRenderQueue();
            if (_checkersBrain.IsAiTurn)
            {
                RunAiTurn();
            }
            else
            {
                shouldQuit = HandlePlayerInput();
            }

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