using System.Collections;
using Domain;

namespace GameBrain;

public record GamePieceWithPosition(GamePiece GamePiece, int X, int Y);

public record Move(int FromX, int FromY, int ToX, int ToY, List<GamePieceWithPosition> GamePieces);

public class CheckersBrain
{
    private readonly CheckersRuleset _checkersRuleset;
    private readonly CheckersGame _checkersGame;
    public int Width => _checkersRuleset.Width;
    public int Height => _checkersRuleset.Height;

    private readonly GamePiece?[,] _pieces;

    public static bool IsSquareBlack(int x, int y)
    {
        return (x + y) % 2 != 0;
    }

    public GamePiece? this[int x, int y] => _pieces[x, y];

    private string? WhitePlayerName => _checkersGame.WhitePlayerName;
    private string? BlackPlayerName => _checkersGame.BlackPlayerName;

    public EPlayerColor? Winner => _checkersGame.Winner;
    public bool Tied => _checkersGame.Tied;
    public bool Ended => _checkersGame.Ended;

    public CheckersBrain(CheckersGame checkersGame)
    {
        _checkersGame = checkersGame;

        var checkersRuleset = checkersGame.CheckersRuleset!;
        _checkersRuleset = checkersRuleset;

        if (_checkersGame.CurrentCheckersState != null)
        {
            var currentCheckersState = _checkersGame.CurrentCheckersState;
            _pieces = currentCheckersState.GamePieces!;
            WhiteMoves = currentCheckersState.WhiteMoves;
            BlackMoves = currentCheckersState.BlackMoves;
            LastMovedToX = currentCheckersState.LastMovedToX;
            LastMovedToY = currentCheckersState.LastMovedToY;
            LastMoveState = currentCheckersState.LastMoveState;
        }
        else
        {
            _pieces = new GamePiece?[checkersRuleset.Width, checkersRuleset.Height];
            InitializePieces();
        }
    }

    public CheckersBrain(CheckersRuleset checkersRuleset,
        string? whitePlayerName, string? blackPlayerName,
        EAiType? whiteAiType = null, EAiType? blackAiType = null)
    {
        _checkersGame = new CheckersGame
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
        {
            for (var y = rowsStart; y < rowsEnd; y++)
            {
                if (IsSquareBlack(x, y))
                {
                    _pieces[x, y] = new GamePiece(playerColor);
                }
            }
        }
    }

    private void InitializePieces()
    {
        var rowsPerPlayer = RowsPerPlayer();
        InitializePlayerPieces(0, rowsPerPlayer, EPlayerColor.White);
        InitializePlayerPieces(Height - rowsPerPlayer, Height, EPlayerColor.Black);
    }

    public string? PlayerName(EPlayerColor playerColor)
    {
        return playerColor switch
        {
            EPlayerColor.Black => BlackPlayerName,
            EPlayerColor.White => WhitePlayerName,
            _ => throw new ArgumentException($"Invalid player color {playerColor}")
        };
    }

    public int WhiteMoves { get; set; }
    public int BlackMoves { get; set; }
    public int? LastMovedToX { get; set; }
    public int? LastMovedToY { get; set; }
    public EMoveState? LastMoveState { get; set; } = EMoveState.Finished;

    private List<Move>[,]? CalculatedMoves { get; set; }
    private int? CalculatedMovesCount { get; set; }
    private List<Move>[,]? CalculatedCapturingMoves { get; set; }
    private int? CalculatedCapturingMovesCount { get; set; }

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

    private bool MovesCalculated => CalculatedMoves != null && CalculatedMovesCount != null;
    private bool CapturingMovesCalculated => CalculatedCapturingMoves != null && CalculatedCapturingMovesCount != null;

    public bool IsPlayerTurn(EPlayerColor playerColor) => playerColor == CurrentTurnPlayerColor;

    public EPlayerColor CurrentTurnPlayerColor
    {
        get
        {
            var firstPlayer = _checkersRuleset.BlackMovesFirst ? EPlayerColor.Black : EPlayerColor.White;
            var otherPlayer = _checkersRuleset.BlackMovesFirst ? EPlayerColor.White : EPlayerColor.Black;
            var firstPlayerMoves = _checkersRuleset.BlackMovesFirst ? BlackMoves : WhiteMoves;
            var otherPlayerMoves = _checkersRuleset.BlackMovesFirst ? WhiteMoves : BlackMoves;

            if (firstPlayerMoves == otherPlayerMoves) return firstPlayer;
            if (firstPlayerMoves == otherPlayerMoves + 1) return otherPlayer;
            throw new Exception(
                $"Illegal difference between player move counts! BlackMoves: {BlackMoves}, WhiteMoves: {WhiteMoves}, BlackMovesFirst: {_checkersRuleset.BlackMovesFirst}");
        }
    }

    public bool IsAiTurn => CurrentTurnAiType != null;

    private ICheckersAi? CurrentTurnAi =>
        CurrentTurnAiType != null ? CheckersAiContext.GetCheckersAi(CurrentTurnAiType.Value) : null;

    private EAiType? CurrentTurnAiType
    {
        get
        {
            return CurrentTurnPlayerColor switch
            {
                EPlayerColor.Black => _checkersGame.BlackAiType,
                EPlayerColor.White => _checkersGame.WhiteAiType,
                _ => null
            };
        }
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

        if (_pieces[fromX, fromY] == null) return result;
        var gamePiece = _pieces[fromX, fromY]!.Value;

        if (!IsPieceMovableBasic(gamePiece.Player, fromX, fromY)) return result;

        var isContinuingTurn =
            LastMovedToX == fromX && LastMovedToY == fromY && LastMoveState == EMoveState.CanContinue;

        var increments = new List<int> { -1, 1 };
        foreach (var yIncrement in increments)
        {
            foreach (var xIncrement in increments)
            {
                var x = fromX + xIncrement;
                var y = fromY + yIncrement;
                var c = 1;
                var gamePiecesOnPath = new List<GamePieceWithPosition>();
                while (x >= 0 && x < Width && y >= 0 && y < Height &&
                       (c <= 2 || (gamePiece.IsCrowned && _checkersRuleset.FlyingKings)) &&
                       gamePiecesOnPath.Count < 2)
                {
                    var gamePieceOnPath = _pieces[x, y];
                    if (gamePieceOnPath != null)
                    {
                        if (gamePieceOnPath.Value.Player == gamePiece.Player) break;
                        gamePiecesOnPath.Add(new GamePieceWithPosition(gamePieceOnPath.Value, x, y));
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
               gamePiece.IsCrowned && (distance == 1 || (distance == 2 && isCapturing)) ||
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
        if (Ended) return;
        for (var fromX = 0; fromX < Width; fromX++)
        {
            for (var fromY = 0; fromY < Height; fromY++)
            {
                var moves = CalculateAvailableMoves(fromX, fromY);
                CalculatedMoves[fromX, fromY] = moves;
                CalculatedMovesCount += moves.Count;
            }
        }
    }

    private void CalculateAllCapturingMoves()
    {
        if (!MovesCalculated) CalculateAllAvailableMoves();
        CalculatedCapturingMoves = new List<Move>[Width, Height];
        CalculatedCapturingMovesCount = 0;
        if (Ended) return;
        for (var fromX = 0; fromX < Width; fromX++)
        {
            for (var fromY = 0; fromY < Height; fromY++)
            {
                var moves = CalculatedMoves![fromX, fromY];
                var capturingMoves = moves.FindAll(move => move.GamePieces.Count > 0);
                CalculatedCapturingMoves[fromX, fromY] = capturingMoves;
                CalculatedCapturingMovesCount += capturingMoves.Count;
            }
        }
    }

    public List<Move> AvailableMoves()
    {
        var result = new List<Move>();
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                result.AddRange(AvailableMoves(x, y));
            }
        }

        return result;
    }

    public List<Move>
        AvailableMoves(int fromX, int fromY)
    {
        if (!MovesCalculated) CalculateAllAvailableMoves();
        if (!_checkersRuleset.MustCapture) return CalculatedMoves![fromX, fromY];
        if (!CapturingMovesCalculated) CalculateAllCapturingMoves();
        return CalculatedCapturingMovesCount > 0
            ? CalculatedCapturingMoves![fromX, fromY]
            : CalculatedMoves![fromX, fromY];
    }

    public bool IsPieceMovableBasic(EPlayerColor playerColor, int x, int y)
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
        foreach (var capturedGamePiece in move.GamePieces)
        {
            _pieces[capturedGamePiece.X, capturedGamePiece.Y] = null;
        }

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
    }

    public void MoveAi()
    {
        if (!IsAiTurn) throw new NotAllowedException("Can't make AI move during player turn!");
        if (CurrentTurnAi == null)
            throw new IllegalStateException($"Can't make AI move - no AI found of type '{CurrentTurnAiType}'");
        var ai = CurrentTurnAi;
        ai.Move(this);
    }

    public void IncrementMoveCounter()
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

    public bool EndTurnAllowed => LastMoveState == EMoveState.CanContinue && !_checkersRuleset.MustCapture;

    private (int BlackPieces, int WhitePieces) PieceCounts
    {
        get
        {
            var result = (0, 0);
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var gamePiece = _pieces[x, y];
                    if (gamePiece?.Player == EPlayerColor.Black) result.Item1 += 1;
                    if (gamePiece?.Player == EPlayerColor.White) result.Item2 += 1;
                }
            }

            return result;
        }
    }

    public void EndTurn()
    {
        if (!EndTurnAllowed)
            throw new NotAllowedException("Not allowed to end turn!");

        IncrementMoveCounter();

        LastMoveState = EMoveState.Finished;
        
        CheckGameEndConditions();
    }

    private void CheckGameEndConditions()
    {
        if (Ended) return;
        var pieceCounts = PieceCounts;
        if (pieceCounts.WhitePieces == 0)
        {
            _checkersGame.Winner = EPlayerColor.Black;
            _checkersGame.EndedAt = DateTime.Now.ToUniversalTime();
        }
        if (pieceCounts.BlackPieces == 0)
        {
            _checkersGame.Winner = EPlayerColor.White;
            _checkersGame.EndedAt = DateTime.Now.ToUniversalTime();
        }
    }

    private CheckersState GetCheckersState()
    {
        var gamePiecesCopy = new GamePiece?[_pieces.GetLength(0), _pieces.GetLength(1)];
        for (var y = 0; y < _pieces.GetLength(1); y++)
        {
            for (var x = 0; x < _pieces.GetLength(0); x++)
            {
                var gamePiece = _pieces[x, y];
                gamePiecesCopy[x, y] = gamePiece != null
                    ? new GamePiece(gamePiece.Value.Player, gamePiece.Value.IsCrowned)
                    : null;
            }
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

    public CheckersGame GetSaveGameState()
    {
        if (_checkersGame.CheckersStates == null) throw new InsufficientCheckersStatesException(_checkersGame);
        _checkersGame.CheckersStates.Add(GetCheckersState());

        return _checkersGame;
    }
}