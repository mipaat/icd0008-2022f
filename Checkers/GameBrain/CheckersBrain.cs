using System.Collections;
using Common;
using Domain.Model;
using Domain.Model.Helpers;
using GameBrain.AI;

namespace GameBrain;

public record GamePiecePosition(int X, int Y);

public record Move(int FromX, int FromY, int ToX, int ToY, List<GamePiecePosition> GamePieces);

public class CheckersBrain
{
    private readonly CheckersRuleset _checkersRuleset;

    private readonly GamePiece?[,] _pieces;

    public CheckersBrain(CheckersGame checkersGame)
    {
        CheckersGame = checkersGame;

        var checkersRuleset = checkersGame.CheckersRuleset!;
        _checkersRuleset = checkersRuleset;

        _pieces = new GamePiece?[checkersRuleset.Width, checkersRuleset.Height];
        if (CheckersGame.CurrentCheckersState != null)
        {
            var currentCheckersState = CheckersGame.CurrentCheckersState;
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                _pieces[x, y] = currentCheckersState.GamePieces![x, y];

            WhiteMoves = currentCheckersState.WhiteMoves;
            BlackMoves = currentCheckersState.BlackMoves;
            LastMovedToX = currentCheckersState.LastMovedToX;
            LastMovedToY = currentCheckersState.LastMovedToY;
            LastMoveState = currentCheckersState.LastMoveState;
        }
        else
        {
            InitializePieces();
        }

        CheckGameEndConditions();
    }

    public CheckersBrain(CheckersRuleset checkersRuleset,
        string? whitePlayerName, string? blackPlayerName,
        EAiType? whiteAiType = null, EAiType? blackAiType = null)
    {
        CheckersGame = new CheckersGame
        {
            WhitePlayerName = whitePlayerName,
            BlackPlayerName = blackPlayerName,
            WhiteAiType = whiteAiType,
            BlackAiType = blackAiType,
            CheckersRuleset = checkersRuleset,
            CheckersStates = new List<CheckersState>(),
            StartedAt = DateTime.Now.ToUniversalTime()
        };

        _checkersRuleset = checkersRuleset;
        _pieces = new GamePiece?[checkersRuleset.Width, checkersRuleset.Height];
        InitializePieces();
        SaveGameState();
    }

    public CheckersGame CheckersGame { get; }
    public int Width => _checkersRuleset.Width;
    public int Height => _checkersRuleset.Height;

    public GamePiece? this[int x, int y] => _pieces[x, y];

    public Player WhitePlayer => CheckersGame.WhitePlayer;
    public Player BlackPlayer => CheckersGame.BlackPlayer;
    public Player? Winner => CheckersGame.WinnerPlayer;
    public bool Tied => CheckersGame.Tied;
    public bool Ended => CheckersGame.Ended;

    public int WhiteMoves { get; private set; }
    public int BlackMoves { get; private set; }
    public int? LastMovedToX { get; private set; }
    public int? LastMovedToY { get; private set; }
    public EMoveState? LastMoveState { get; private set; } = EMoveState.Finished;

    private List<Move>[,]? CalculatedMoves { get; set; }
    private int? CalculatedMovesCount { get; set; }
    private List<Move>[,]? CalculatedCapturingMoves { get; set; }
    private int? CalculatedCapturingMovesCount { get; set; }

    private bool MovesCalculated => CalculatedMoves != null && CalculatedMovesCount != null;
    private bool CapturingMovesCalculated => CalculatedCapturingMoves != null && CalculatedCapturingMovesCount != null;

    public Player FirstPlayer => _checkersRuleset.BlackMovesFirst ? BlackPlayer : WhitePlayer;

    public Player CurrentTurnPlayer
    {
        get
        {
            var firstPlayer = FirstPlayer;
            var otherPlayer = OtherPlayer(firstPlayer);
            var firstPlayerMoves = firstPlayer.Color == EPlayerColor.Black ? BlackMoves : WhiteMoves;
            var otherPlayerMoves = otherPlayer.Color == EPlayerColor.Black ? BlackMoves : WhiteMoves;
            if (firstPlayerMoves == otherPlayerMoves) return firstPlayer;
            if (firstPlayerMoves == otherPlayerMoves + 1) return otherPlayer;
            throw new IllegalGameStateException(
                $"Illegal difference between player move counts! BlackMoves: {BlackMoves}, WhiteMoves: {WhiteMoves}, BlackMovesFirst: {_checkersRuleset.BlackMovesFirst}");
        }
    }

    public EPlayerColor CurrentTurnPlayerColor => CurrentTurnPlayer.Color;

    public bool IsAiTurn => CurrentTurnAiType != null;

    private ICheckersAi? CurrentTurnAi =>
        CurrentTurnAiType != null ? CheckersAiContext.GetCheckersAi(CurrentTurnAiType.Value) : null;

    private EAiType? CurrentTurnAiType
    {
        get
        {
            return CurrentTurnPlayerColor switch
            {
                EPlayerColor.Black => CheckersGame.BlackAiType,
                EPlayerColor.White => CheckersGame.WhiteAiType,
                _ => null
            };
        }
    }

    public bool DrawResolutionExpected => DrawResolutionExpectedFrom(CurrentTurnPlayerColor);

    public bool EndTurnAllowed => LastMoveState == EMoveState.CanContinue && !_checkersRuleset.MustCapture;

    public (int BlackPieces, int WhitePieces) PieceCounts
    {
        get
        {
            var result = (0, 0);
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                var gamePiece = _pieces[x, y];
                if (gamePiece?.Player == EPlayerColor.Black) result.Item1 += 1;
                if (gamePiece?.Player == EPlayerColor.White) result.Item2 += 1;
            }

            return result;
        }
    }

    public static bool IsSquareBlack(int x, int y)
    {
        return (x + y) % 2 != 0;
    }

    public Player Player(EPlayerColor playerColor)
    {
        return CheckersGame.Player(playerColor);
    }

    private int RowsPerPlayer()
    {
        return RowsPerPlayer(Height);
    }

    private static int RowsPerPlayer(int height)
    {
        return (height - 2) / 2;
    }

    private void InitializePlayerPieces(int rowsStart, int rowsEnd, EPlayerColor playerColor)
    {
        for (var x = 0; x < Width; x++)
        for (var y = rowsStart; y < rowsEnd; y++)
            if (IsSquareBlack(x, y))
                _pieces[x, y] = new GamePiece(playerColor);
    }

    private void InitializePieces()
    {
        var rowsPerPlayer = RowsPerPlayer();
        InitializePlayerPieces(0, rowsPerPlayer, WhitePlayer.Color);
        InitializePlayerPieces(Height - rowsPerPlayer, Height, BlackPlayer.Color);
    }

    private void ClearCalculatedMoves()
    {
        CalculatedMoves = null;
        CalculatedMovesCount = null;
    }

    private void ClearCalculatedCapturingMoves()
    {
        CalculatedCapturingMoves = null;
        CalculatedCapturingMovesCount = null;
    }

    public bool IsPlayerTurn(EPlayerColor playerColor)
    {
        return playerColor == CurrentTurnPlayerColor;
    }

    public Player OtherPlayer(Player player)
    {
        return CheckersGame.OtherPlayer(player);
    }

    public EPlayerColor OtherPlayer(EPlayerColor playerColor)
    {
        return CheckersGame.OtherPlayer(playerColor);
    }

    public bool DrawResolutionExpectedFrom(EPlayerColor playerColor)
    {
        if (CheckersGame.DrawProposedBy == null) return false;
        if (CheckersGame.Ended) return false;
        return CheckersGame.DrawProposedBy == OtherPlayer(playerColor);
    }

    private List<Move> CalculateCapturingMoves(int fromX, int fromY)
    {
        return CalculateAvailableMoves(fromX, fromY).FindAll(move => move.GamePieces.Count > 0);
    }

    private int BackWardsDirection(GamePiece gamePiece)
    {
        return gamePiece.Player switch
        {
            EPlayerColor.Black => 1,
            EPlayerColor.White => -1,
            _ => throw new IllegalGameStateException(
                $"GamePiece has unknown color: {gamePiece.Player}") // Should never happen
        };
    }

    private List<Move> CalculateAvailableMoves(int fromX, int fromY)
    {
        var result = new List<Move>();

        if (Ended || _pieces[fromX, fromY] == null) return result;
        var gamePiece = _pieces[fromX, fromY]!.Value;

        if (!IsPieceMovableBasic(gamePiece.Player, fromX, fromY)) return result;

        var isContinuingTurn =
            LastMovedToX == fromX && LastMovedToY == fromY && LastMoveState == EMoveState.CanContinue;

        var increments = new List<int> { -1, 1 };
        foreach (var yIncrement in increments)
        foreach (var xIncrement in increments)
        {
            var x = fromX + xIncrement;
            var y = fromY + yIncrement;
            var c = 1;
            var gamePiecesOnPath = new List<GamePiecePosition>();
            while (x >= 0 && x < Width && y >= 0 && y < Height &&
                   (c <= 2 || (gamePiece.IsCrowned && _checkersRuleset.FlyingKings)) &&
                   gamePiecesOnPath.Count < 2)
            {
                var gamePieceOnPath = _pieces[x, y];
                if (gamePieceOnPath != null)
                {
                    if (gamePieceOnPath.Value.Player == gamePiece.Player) break;
                    gamePiecesOnPath.Add(new GamePiecePosition(x, y));
                }
                else
                {
                    if (CheckValid(gamePiece, gamePiecesOnPath, c, yIncrement, isContinuingTurn))
                        result.Add(new Move(fromX, fromY, x, y, gamePiecesOnPath.ToList()));
                }

                x += xIncrement;
                y += yIncrement;
                c++;
            }
        }

        return result;
    }

    private bool CheckValid(GamePiece gamePiece, ICollection gamePiecesOnPath, int distance, int yIncrement,
        bool isContinuingTurn)
    {
        var isCapturing = gamePiecesOnPath.Count > 0;
        return CheckDistanceValid(gamePiece, distance, isCapturing)
               && CheckContinueValid(gamePiecesOnPath)
               && CheckBackwardsValid(yIncrement, gamePiece, isCapturing, isContinuingTurn);
    }

    private bool CheckDistanceValid(GamePiece gamePiece, int distance, bool isCapturing)
    {
        return (!gamePiece.IsCrowned && (distance == 1 || (distance == 2 && isCapturing))) ||
               (gamePiece.IsCrowned && (distance == 1 || (distance == 2 && isCapturing))) ||
               (_checkersRuleset.FlyingKings && gamePiece.IsCrowned && distance > 0);
    }

    private bool CheckContinueValid(ICollection gamePiecesOnPath)
    {
        return !(LastMoveState == EMoveState.CanContinue && gamePiecesOnPath.Count == 0);
    }

    private bool CheckBackwardsValid(int yIncrement, GamePiece gamePiece, bool isCapturing, bool isContinuingTurn)
    {
        return yIncrement != BackWardsDirection(gamePiece) ||
               gamePiece.IsCrowned ||
               (isCapturing &&
                (_checkersRuleset.CanCaptureBackwards ||
                 (isContinuingTurn && _checkersRuleset.CanCaptureBackwardsDuringMultiCapture)));
    }

    private void CalculateAllAvailableMoves()
    {
        CalculatedMoves = new List<Move>[Width, Height];
        CalculatedMovesCount = 0;
        for (var fromX = 0; fromX < Width; fromX++)
        for (var fromY = 0; fromY < Height; fromY++)
        {
            var moves = CalculateAvailableMoves(fromX, fromY);
            CalculatedMoves[fromX, fromY] = moves;
            CalculatedMovesCount += moves.Count;
        }
    }

    private void CalculateAllCapturingMoves()
    {
        if (!MovesCalculated) CalculateAllAvailableMoves();
        CalculatedCapturingMoves = new List<Move>[Width, Height];
        CalculatedCapturingMovesCount = 0;
        if (Ended) return;
        for (var fromX = 0; fromX < Width; fromX++)
        for (var fromY = 0; fromY < Height; fromY++)
        {
            var moves = CalculatedMoves![fromX, fromY];
            var capturingMoves = moves.FindAll(move => move.GamePieces.Count > 0);
            CalculatedCapturingMoves[fromX, fromY] = capturingMoves;
            CalculatedCapturingMovesCount += capturingMoves.Count;
        }
    }

    public List<Move> AvailableMoves()
    {
        var result = new List<Move>();
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            result.AddRange(AvailableMoves(x, y));

        return result;
    }

    public List<Move>
        AvailableMoves(int fromX, int fromY)
    {
        if (Ended) return new List<Move>();
        if (!MovesCalculated) CalculateAllAvailableMoves();
        if (!_checkersRuleset.MustCapture) return CalculatedMoves![fromX, fromY];
        if (!CapturingMovesCalculated) CalculateAllCapturingMoves();
        return CalculatedCapturingMovesCount > 0
            ? CalculatedCapturingMoves![fromX, fromY]
            : CalculatedMoves![fromX, fromY];
    }

    private bool IsPieceMovableBasic(EPlayerColor playerColor, int x, int y)
    {
        if (Ended) return false;
        var gamePiece = _pieces[x, y];
        if (gamePiece == null) return false;
        if (LastMovedToX != null && LastMovedToY != null && LastMoveState == EMoveState.CanContinue)
        {
            var previousMoveGamePiece = _pieces[LastMovedToX!.Value, LastMovedToY!.Value];
            if (previousMoveGamePiece == null)
                throw new IllegalGameStateException(
                    $"GamePiece expected at ({LastMovedToX}, {LastMovedToY}) from previous move with state {LastMoveState}!");
            if (previousMoveGamePiece.Value.Player != CurrentTurnPlayerColor)
                throw new IllegalGameStateException(
                    $"Expecting move from {previousMoveGamePiece.Value.Player} during {CurrentTurnPlayerColor}'s turn!");
            if (x != LastMovedToX || y != LastMovedToY) return false;
        }

        return gamePiece.Value.Player == playerColor && playerColor == CurrentTurnPlayerColor;
    }

    public bool IsPieceMovable(EPlayerColor playerColor, int x, int y)
    {
        return IsPieceMovableBasic(playerColor, x, y) && AvailableMoves(x, y).Count > 0;
    }

    public bool IsMoveValid(EPlayerColor playerColor, int sourceX, int sourceY, int destinationX, int destinationY)
    {
        if (Ended) return false;
        if (!IsPieceMovableBasic(playerColor, sourceX, sourceY)) return false;
        var availableMoves = AvailableMoves(sourceX, sourceY);
        return availableMoves.Exists(move => move.ToX == destinationX && move.ToY == destinationY);
    }

    public void Move(int sourceX, int sourceY, int destinationX, int destinationY)
    {
        var availableMoves = AvailableMoves(sourceX, sourceY);
        var move = availableMoves.Find(m => m.ToX == destinationX && m.ToY == destinationY);

        if (move == null)
            throw new NotAllowedException(
                $"Moving from ({sourceX}, {sourceY}) to ({destinationX}, {destinationY}) is not allowed!");

        ClearCalculatedMoves();
        ClearCalculatedCapturingMoves();
        var gamePiece = _pieces[sourceX, sourceY]!.Value;
        if ((gamePiece.Player == EPlayerColor.Black && destinationY == 0) ||
            (gamePiece.Player == EPlayerColor.White && destinationY == Height - 1)) gamePiece.IsCrowned = true;
        _pieces[destinationX, destinationY] = gamePiece;
        _pieces[sourceX, sourceY] = null;
        foreach (var capturedGamePiece in move.GamePieces) _pieces[capturedGamePiece.X, capturedGamePiece.Y] = null;

        LastMovedToX = destinationX;
        LastMovedToY = destinationY;

        LastMoveState = move.GamePieces.Count == 0 || CalculateCapturingMoves(destinationX, destinationY).Count == 0
            ? EMoveState.Finished
            : EMoveState.CanContinue;

        if (LastMoveState == EMoveState.Finished)
        {
            IncrementMoveCounter();
            CheckGameEndConditions();
        }

        SaveGameState();
    }

    public void MoveAi()
    {
        if (!IsAiTurn) throw new NotAllowedException("Can't make AI move during player turn!");
        if (CurrentTurnAi == null)
            throw new IllegalStateException($"Can't make AI move - no AI found of type '{CurrentTurnAiType}'");
        var ai = CurrentTurnAi;
        ai.Move(this);
    }

    private void IncrementMoveCounter()
    {
        switch (CurrentTurnPlayerColor)
        {
            case EPlayerColor.Black:
                BlackMoves++;
                break;
            case EPlayerColor.White:
                WhiteMoves++;
                break;
        }
    }

    public void EndTurn()
    {
        if (!EndTurnAllowed)
            throw new NotAllowedException("Not allowed to end turn!");

        IncrementMoveCounter();

        LastMoveState = EMoveState.Finished;

        CheckGameEndConditions();

        SaveGameState();
    }

    private void CheckGameEndConditions()
    {
        if (Ended) return;
        var pieceCounts = PieceCounts;
        if (pieceCounts.WhitePieces == 0)
        {
            CheckersGame.Winner = EPlayerColor.Black;
            CheckersGame.EndedAt = DateTime.Now.ToUniversalTime();
        }

        if (pieceCounts.BlackPieces == 0)
        {
            CheckersGame.Winner = EPlayerColor.White;
            CheckersGame.EndedAt = DateTime.Now.ToUniversalTime();
        }

        if (AvailableMoves().Count == 0) Forfeit();
    }

    public void Forfeit(EPlayerColor? playerColor = null)
    {
        if (Ended) return;
        CheckersGame.EndedAt = DateTime.UtcNow;
        CheckersGame.Winner = OtherPlayer(playerColor ?? CurrentTurnPlayerColor);
    }

    public void ProposeDraw(EPlayerColor? playerColor = null)
    {
        CheckersGame.DrawProposedBy = playerColor ?? CurrentTurnPlayerColor;
    }

    public void AcceptDraw(EPlayerColor? playerColor = null)
    {
        if (CheckersGame.DrawProposedBy == null)
            throw new NotAllowedException("Can't accept draw - no draw was proposed!");
        if (CheckersGame.DrawProposedBy == (playerColor ?? CurrentTurnPlayerColor))
            throw new NotAllowedException(
                $"Draw can't be accepted by the player that proposed it ({CheckersGame.DrawProposedBy})!");
        Draw();
    }

    public void RejectDraw(EPlayerColor? playerColor = null)
    {
        if (CheckersGame.DrawProposedBy == null)
            throw new NotAllowedException("Can't accept draw - no draw was proposed!");
        if (CheckersGame.DrawProposedBy == (playerColor ?? CurrentTurnPlayerColor))
            throw new NotAllowedException(
                $"Draw can't be accepted by the player that proposed it ({CheckersGame.DrawProposedBy})!");
        CheckersGame.DrawProposedBy = null;
    }

    public void Draw()
    {
        if (Ended) return;
        CheckersGame.EndedAt = DateTime.UtcNow;
    }

    private CheckersState GetCheckersState()
    {
        var gamePiecesCopy = new GamePiece?[_pieces.GetLength(0), _pieces.GetLength(1)];
        for (var y = 0; y < _pieces.GetLength(1); y++)
        for (var x = 0; x < _pieces.GetLength(0); x++)
        {
            var gamePiece = _pieces[x, y];
            gamePiecesCopy[x, y] = gamePiece != null
                ? new GamePiece(gamePiece.Value.Player, gamePiece.Value.IsCrowned)
                : null;
        }

        return new CheckersState
        {
            CreatedAt = DateTime.Now.ToUniversalTime(),
            GamePieces = gamePiecesCopy,
            WhiteMoves = WhiteMoves,
            BlackMoves = BlackMoves,
            LastMovedToX = LastMovedToX,
            LastMovedToY = LastMovedToY,
            LastMoveState = LastMoveState
        };
    }

    private void SaveGameState()
    {
        CheckersGame.CheckersStates!.Add(GetCheckersState());
    }
}