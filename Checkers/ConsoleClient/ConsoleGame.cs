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

    private static ConsoleKeyInfoBasic QuitButton { get; } = new(ConsoleKey.Q);

    private void Refresh()
    {
        var checkersGame = _checkersBrain.CheckersGame;
        _repositoryContext.CheckersGameRepository.Refresh(checkersGame);
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

    private void ReRenderBoard()
    {
        _consoleWindow.ClearRenderQueue();
        AddBoardToRenderQueue();
        _consoleWindow.Render();
    }

    private static bool IsQuitInput(ConsoleInput input)
    {
        return input.KeyInfo is { Key: ConsoleKey.Q, Modifiers: ConsoleModifiers.Control } || QuitButton.Equals(input.KeyInfo);
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
                    ReRenderBoard();
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
                                               e.ToString().ReplaceLineEndings("   ").Replace('\r', '<'));
            }
        }
    }

    private void RunAiTurn()
    {
        AddBoardToRenderQueue();
        _consoleWindow.Render();
        var timer = new System.Timers.Timer(1000);
        var timerElapsed = false;
        timer.Elapsed += (_, _) => timerElapsed = true;
        var aiColor = _checkersBrain.CurrentTurnPlayerColor;
        timer.Start();
        while (_checkersBrain.CurrentTurnPlayerColor == aiColor && !_checkersBrain.Ended)
        {
            _checkersBrain.MoveAi();
        }

        while (!timerElapsed)
        {
        }

        _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
    }

    private bool DrawResolutionRequired => _checkersBrain.CheckersGame.DrawProposedBy ==
                                           _checkersBrain.OtherPlayer(_checkersBrain.CurrentTurnPlayerColor);

    private bool HandlePlayerTurn()
    {
        var controls = "Quit: Q, Forfeit: F";
        if (_checkersBrain.EndTurnAllowed) controls += ", End turn: E";
        if (_checkersBrain.CheckersGame.DrawProposedBy == null)
        {
            controls += ", Propose Draw: D";
        }

        _consoleWindow.AddLine(controls);

        if (DrawResolutionRequired)
        {
            _consoleWindow.AddLine("Accept opponent's proposal to end game with draw: D");
        }

        AddBoardToRenderQueue();
        var prompt = "Type coordinate pairs (e.g A4D7) to move!";

        var input = _consoleWindow
            .RenderAndAwaitTextInput(
                prompt,
                keepRenderQueue: true);
        var normalizedInputText = input.Text.Trim().ToLower();

        if (normalizedInputText == "q")
        {
            return true;
        }

        var expectedCurrentTurnPlayerColor = _checkersBrain.CurrentTurnPlayerColor;
        var previousDrawProposer = _checkersBrain.CheckersGame.DrawProposedBy;
        Refresh();
        if (expectedCurrentTurnPlayerColor != _checkersBrain.CurrentTurnPlayerColor)
            throw new IllegalStateException(
                $"Game turn changed unexpectedly from {expectedCurrentTurnPlayerColor} to {_checkersBrain.CurrentTurnPlayerColor}!");

        if (_checkersBrain.Ended) return false;
        if (previousDrawProposer != _checkersBrain.CheckersGame.DrawProposedBy) return HandlePlayerTurn();

        if (normalizedInputText == "d")
        {
            if (_checkersBrain.CheckersGame.DrawProposedBy ==
                _checkersBrain.OtherPlayer(_checkersBrain.CurrentTurnPlayerColor))
            {
                _checkersBrain.AcceptDraw();
            }
            else
            {
                _checkersBrain.ProposeDraw();
            }

            _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
            return false;
        }

        if (DrawResolutionRequired)
        {
            _checkersBrain.RejectDraw();
            _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
        }

        if (normalizedInputText == "e")
        {
            if (_checkersBrain.EndTurnAllowed)
            {
                _checkersBrain.EndTurn();
                ReRenderBoard();
                _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
            }
            else
            {
                _consoleWindow.PopUpMessageBox("Ending turn is not allowed!");
            }

            return false;
        }

        if (normalizedInputText == "f")
        {
            _checkersBrain.Forfeit();
            _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
            return false;
        }

        MakePlayerMove(input);
        return false;
    }

    private bool AwaitOtherPlayerMove()
    {
        var forfeitInput = new ConsoleKeyInfoBasic(ConsoleKey.F);
        var timer = new System.Timers.Timer(2000);
        var timerElapsed = false;
        timer.Elapsed += (_, _) => timerElapsed = true;
        timer.Start();
        while (!timerElapsed)
        {
            _consoleWindow.AddLine("Waiting for opponent to move");
            _consoleWindow.AddLine("Press Q if you want to quit");
            AddBoardToRenderQueue();
            _consoleWindow.Render();

            var input = _consoleWindow.AwaitInput(100);

            if (IsQuitInput(input))
            {
                timer.Stop();
                return true;
            }

            if (forfeitInput.Equals(input.KeyInfo))
            {
                timer.Stop();
                _checkersBrain.Forfeit(_playMode);
                _repositoryContext.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
                return false;
            }
        }

        timer.Stop();
        return false;
    }

    private bool ShowEndScreen()
    {
        while (true)
        {
            _consoleWindow.AddLine("Game ended!");
            _consoleWindow.AddLine(_checkersBrain.Tied ? "TIED" : $"Winner: {_checkersBrain.Winner}");
            _consoleWindow.AddLine("Press Q to quit!");
            _consoleWindow.AddLine();
            AddBoardToRenderQueue();
            _consoleWindow.Render();
            var input = _consoleWindow.AwaitInput(null, false, QuitButton);
            if (IsQuitInput(input)) return true;
        }
    }

    public void Run()
    {
        var shouldQuit = false;
        while (!shouldQuit)
        {
            try
            {
                Refresh();
                if (_checkersBrain.Ended)
                {
                    shouldQuit = ShowEndScreen();
                }
                else
                {
                    if (_checkersBrain.IsAiTurn)
                    {
                        RunAiTurn();
                    }
                    else
                    {
                        if (_playMode == _checkersBrain.CurrentTurnPlayerColor || _playMode == null)
                        {
                            shouldQuit = HandlePlayerTurn();
                        }
                        else
                        {
                            shouldQuit = AwaitOtherPlayerMove();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _consoleWindow.PopUpMessageBox($"Error: {e.GetType()} - {e.Message}");
            }
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