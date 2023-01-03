using System.Text;
using System.Text.RegularExpressions;
using Common;
using ConsoleUI;
using DAL;
using Domain.Model.Helpers;
using GameBrain;
using Timer = System.Timers.Timer;

namespace ConsoleClient;

public class ConsoleGame
{
    private readonly ConsoleWindow _consoleWindow;
    private readonly EPlayerColor? _playMode;
    private readonly IRepositoryContextFactory _repositoryContextFactory;
    private IRepositoryContext GetRepoCtx() => _repositoryContextFactory.CreateRepositoryContext();
    private CheckersBrain _checkersBrain;

    public ConsoleGame(ConsoleWindow consoleWindow, CheckersBrain checkersBrain,
        IRepositoryContextFactory repositoryContextFactory, EPlayerColor? playMode = null)
    {
        _checkersBrain = checkersBrain;
        _consoleWindow = consoleWindow;
        _repositoryContextFactory = repositoryContextFactory;
        _playMode = playMode;
    }

    private static ConsoleKeyInfoBasic QuitButton { get; } = new(ConsoleKey.Q);

    private void Refresh()
    {
        using var repoCtx = GetRepoCtx();
        var checkersGame = _checkersBrain.CheckersGame;
        var fetchedCheckersGame = repoCtx.CheckersGameRepository.GetById(checkersGame.Id);
        if (fetchedCheckersGame == null) throw new IllegalStateException("CheckersGame was null after refresh!");
        _checkersBrain = new CheckersBrain(fetchedCheckersGame);
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
        return input.KeyInfo is { Key: ConsoleKey.Q, Modifiers: ConsoleModifiers.Control } ||
               QuitButton.Equals(input.KeyInfo);
    }

    private void MakePlayerMove(ConsoleInput input, IRepositoryContext repoCtx)
    {
        var rx = new Regex(@"([A-Za-z]+)(\d+)([A-Za-z]+)(\d+)");
        var matches = rx.Matches(input.Text);
        if (matches.Count == 1)
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
                    repoCtx.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
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

    private Player? ThisPlayer
    {
        get
        {
            var result = _playMode != null ? _checkersBrain.Player(_playMode.Value) : null;
            result ??= _checkersBrain.BlackPlayer.IsAi ^ _checkersBrain.WhitePlayer.IsAi
                ? _checkersBrain.BlackPlayer.IsAi ? _checkersBrain.WhitePlayer : _checkersBrain.BlackPlayer
                : null;
            return result;
        }
    }

    private bool ForceDrawAllowed =>
        _checkersBrain.WhitePlayer.IsAi || _checkersBrain.BlackPlayer.IsAi || _playMode == null;

    private bool DrawResolutionRequired => ThisPlayer != null
        ? _checkersBrain.DrawResolutionExpectedFrom(ThisPlayer.Color)
        : _checkersBrain.DrawResolutionExpected;

    private string GetDrawControlsText()
    {
        if (ForceDrawAllowed) return "End game with draw: D";
        if (DrawResolutionRequired) return "Accept opponent's proposal to end game with draw: D";
        if (_checkersBrain.CheckersGame.DrawProposedBy == null) return "Offer to end game with draw: D";
        return "";
    }

    private void AddCommonInfoHeaderToRenderQueue()
    {
        _consoleWindow.AddLine($"{_checkersBrain.WhitePlayer} VS {_checkersBrain.BlackPlayer}");
        var controls = "Quit: Q";
        if (!(_checkersBrain.BlackPlayer.IsAi && _checkersBrain.WhitePlayer.IsAi)) controls += ", Forfeit: F";
        controls += ", " + GetDrawControlsText();

        if (ThisPlayer != null)
        {
            _consoleWindow.AddLine($"You are: {ThisPlayer}");
            if (_checkersBrain.CurrentTurnPlayer == ThisPlayer)
            {
                _consoleWindow.AddLine("Your turn!");
                if (_checkersBrain.EndTurnAllowed) controls += ", End turn: E";
                _consoleWindow.AddLine(controls);
            }
            else
            {
                _consoleWindow.AddLine("Opponent's turn, waiting...");
                _consoleWindow.AddLine(controls);
            }
        }
        else
        {
            _consoleWindow.AddLine($"Current turn: {_checkersBrain.CurrentTurnPlayer}");
            _consoleWindow.AddLine(controls);
        }
    }

    private bool AwaitOtherTurnEnd(ref bool timerElapsed, Timer timer, bool refreshBoard)
    {
        var forfeitInput = new ConsoleKeyInfoBasic(ConsoleKey.F);
        var drawInput = new ConsoleKeyInfoBasic(ConsoleKey.D);

        while (!timerElapsed)
        {
            if (refreshBoard)
            {
                AddCommonInfoHeaderToRenderQueue();
                AddBoardToRenderQueue();
                _consoleWindow.Render();
            }

            var input = _consoleWindow.AwaitKeyInput(100);

            if (IsQuitInput(input))
            {
                timer.Stop();
                return true;
            }

            if (forfeitInput.Equals(input.KeyInfo))
            {
                timer.Stop();
                _checkersBrain.Forfeit(_playMode);
                using var repoCtx = GetRepoCtx();
                repoCtx.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
                return false;
            }

            if (drawInput.Equals(input.KeyInfo))
            {
                timer.Stop();
                HandleDrawInput();
                return false;
            }
        }

        timer.Stop();
        return false;
    }

    private bool RunAiTurn()
    {
        AddCommonInfoHeaderToRenderQueue();
        AddBoardToRenderQueue();
        _consoleWindow.Render();

        var aiColor = _checkersBrain.CurrentTurnPlayerColor;

        var timer = new Timer(2000);
        var timerElapsed = false;
        timer.Elapsed += (_, _) => timerElapsed = true;
        timer.Start();

        Task.Run(() =>
            {
                var checkersBrain = _checkersBrain;
                while (checkersBrain.CurrentTurnPlayerColor == aiColor && !checkersBrain.Ended && !timerElapsed)
                {
                    checkersBrain.MoveAi();
                    if (!timerElapsed)
                    {
                        using var repoCtx = GetRepoCtx();
                        repoCtx.CheckersGameRepository.Upsert(checkersBrain.CheckersGame);
                    }
                }
            }
        );

        return AwaitOtherTurnEnd(ref timerElapsed, timer, false);
    }

    private void HandleDrawInput()
    {
        using var repoCtx = GetRepoCtx();

        if (ForceDrawAllowed)
        {
            _checkersBrain.Draw();
        }
        else if (DrawResolutionRequired)
        {
            _checkersBrain.AcceptDraw(ThisPlayer?.Color);
        }
        else if (_checkersBrain.CheckersGame.DrawProposedBy == null)
        {
            _checkersBrain.ProposeDraw(ThisPlayer?.Color);
        }

        repoCtx.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
    }

    private bool HandlePlayerTurn()
    {
        AddCommonInfoHeaderToRenderQueue();
        AddBoardToRenderQueue();

        var prompt = "Type coordinate pairs (e.g A4D7) to move!";
        var input = _consoleWindow
            .RenderAndAwaitTextInput(
                prompt,
                true);
        var normalizedInputText = input.Text.Trim().ToLower();

        if (normalizedInputText == "q") return true;

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
            HandleDrawInput();
            return false;
        }

        using var repoCtx = GetRepoCtx();

        if (DrawResolutionRequired)
        {
            _checkersBrain.RejectDraw();
            repoCtx.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
        }

        if (normalizedInputText == "e")
        {
            if (_checkersBrain.EndTurnAllowed)
            {
                _checkersBrain.EndTurn();
                ReRenderBoard();
                repoCtx.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
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
            repoCtx.CheckersGameRepository.Upsert(_checkersBrain.CheckersGame);
            return false;
        }

        MakePlayerMove(input, repoCtx);
        return false;
    }

    private bool AwaitOtherPlayerMove()
    {
        var timer = new Timer(2000);
        var timerElapsed = false;
        timer.Elapsed += (_, _) => timerElapsed = true;
        timer.Start();

        return AwaitOtherTurnEnd(ref timerElapsed, timer, true);
    }

    private bool ShowEndScreen()
    {
        while (true)
        {
            _consoleWindow.AddLine($"Game ended after {_checkersBrain.WhiteMoves + _checkersBrain.BlackMoves} moves!");
            _consoleWindow.AddLine(_checkersBrain.Tied
                ? $"{_checkersBrain.BlackPlayer} TIED with {_checkersBrain.WhitePlayer}"
                : $"WINNER: {_checkersBrain.Winner}, LOSER: {_checkersBrain.OtherPlayer(_checkersBrain.Winner!)}");
            _consoleWindow.AddLine("Press Q to quit!");
            _consoleWindow.AddLine();
            AddBoardToRenderQueue();
            _consoleWindow.Render();
            var input = _consoleWindow.AwaitKeyInput();
            if (IsQuitInput(input)) return true;
        }
    }

    public void Run()
    {
        var shouldQuit = false;
        while (!shouldQuit)
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
                        shouldQuit = RunAiTurn();
                    }
                    else
                    {
                        if (_playMode == _checkersBrain.CurrentTurnPlayerColor || _playMode == null)
                            shouldQuit = HandlePlayerTurn();
                        else
                            shouldQuit = AwaitOtherPlayerMove();
                    }
                }
            }
            catch (Exception e)
            {
                _consoleWindow.PopUpMessageBox($"Error: {e.GetType()} - {e.Message}");
            }
            finally
            {
                _consoleWindow.ClearRenderQueue();
            }
    }

    private static string BoundaryLine(int cells)
    {
        var result = new StringBuilder();
        for (var i = 0; i < cells; i++) result.Append("+---");

        result.Append("+");
        return result.ToString();
    }
}