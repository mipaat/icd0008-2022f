using Domain;

namespace GameBrain;

internal record GamePieceWithPosition(GamePiece GamePiece, int X, int Y);

internal record Move(int ToX, int ToY, List<GamePieceWithPosition> GamePieces);

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

    private string? WhitePlayerId => _checkersGame.WhitePlayerId;
    private string? BlackPlayerId => _checkersGame.BlackPlayerId;

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
            GameElapsedTime = currentCheckersState.GameElapsedTime;
            MoveElapsedTime = currentCheckersState.MoveElapsedTime;
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

    public CheckersBrain(CheckersRuleset checkersRuleset, string whitePlayerId, string blackPlayerId)
    {
        if (whitePlayerId == blackPlayerId)
            throw new ArgumentException($"Player IDs must not be identical! '{whitePlayerId}' = '{blackPlayerId}'");
        _checkersGame = new CheckersGame
        {
            WhitePlayerId = whitePlayerId,
            BlackPlayerId = blackPlayerId,
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

    public EPlayerColor PlayerColor(string? playerId)
    {
        if (playerId == WhitePlayerId)
        {
            return EPlayerColor.White;
        }

        if (playerId == BlackPlayerId)
        {
            return EPlayerColor.Black;
        }

        throw new KeyNotFoundException($"Player with ID {playerId} not found in current game!");
    }

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public TimeSpan MoveElapsedTime { get; set; }
    public TimeSpan GameElapsedTime { get; set; }
    public int WhiteMoves { get; set; }
    public int BlackMoves { get; set; }
    public int? LastMovedToX { get; set; }
    public int? LastMovedToY { get; set; }
    public EMoveState? LastMoveState { get; set; } = EMoveState.Finished;

    public bool IsPlayerTurn(string? playerId)
    {
        try
        {
            var playerColor = PlayerColor(playerId);
            return IsPlayerTurn(playerColor);
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

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

    private List<Move>
        AvailableMoves(int fromX, int fromY)
    {
        var result = new List<Move>();

        if (_pieces[fromX, fromY] == null) return result;
        var gamePiece = _pieces[fromX, fromY]!.Value;

        if (!IsPieceMovableBasic(gamePiece.Player, fromX, fromY)) return result;

        // TODO: Figure out how and where to handle required captures
        // TODO: Maybe save currently calculated availableMoves to not have to recalculate them each time?
        // TODO: Handle crowned pieces and backwards moves
        var increments = new List<int> { -1, 1 };
        foreach (var yIncrement in increments)
        {
            foreach (var xIncrement in increments)
            {
                var x = fromX + xIncrement;
                var y = fromY + yIncrement;
                var c = 1;
                var gamePiecesOnPath = new List<GamePieceWithPosition>();
                while (x >= 0 && x < Width && y >= 0 && y < Height && c <= 2)
                {
                    var gamePieceOnPath = _pieces[x, y];
                    if (gamePieceOnPath != null)
                    {
                        if (gamePieceOnPath.Value.Player == gamePiece.Player) break;
                        gamePiecesOnPath.Add(new GamePieceWithPosition(gamePieceOnPath.Value, x, y));
                    }
                    else
                    {
                        if (!gamePiece.IsCrowned)
                        {
                            if (c == 1 || (c == 2 && gamePiecesOnPath.Count == 1))
                            {
                                result.Add(new Move(x, y, gamePiecesOnPath.ToList()));
                            }
                        }
                        else
                        {
                            result.Add(new Move(x, y, gamePiecesOnPath.ToList()));
                        }
                    }

                    x += xIncrement;
                    y += yIncrement;
                    c++;
                }
            }
        }

        return result;
    }

    public bool IsPieceMovableBasic(EPlayerColor playerColor, int x, int y)
    {
        var gamePiece = _pieces[x, y];
        if (gamePiece == null) return false;
        if (LastMovedToX != null && LastMovedToY != null && LastMoveState == EMoveState.CanContinue)
        {
            var previousMoveGamePiece = this[LastMovedToX!.Value, LastMovedToY!.Value];
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

    public bool IsPieceMovable(string? playerId, int x, int y)
    {
        try
        {
            return IsPieceMovable(PlayerColor(playerId), x, y);
        }
        catch (KeyNotFoundException)
        {
            Console.Out.WriteLine("KEY NOT FOUND " + playerId);
            return false;
        }
    }

    public bool IsPieceMovable(EPlayerColor playerColor, int x, int y)
    {
        return IsPieceMovableBasic(playerColor, x, y) && AvailableMoves(x, y).Count > 0;
    }

    public bool IsMoveValid(string? playerId, int sourceX, int sourceY, int destinationX, int destinationY)
    {
        try
        {
            return IsMoveValid(PlayerColor(playerId), sourceX, sourceY, destinationX, destinationY);
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public bool IsMoveValid(EPlayerColor playerColor, int sourceX, int sourceY, int destinationX, int destinationY)
    {
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

        _pieces[destinationX, destinationY] = _pieces[sourceX, sourceY];
        _pieces[sourceX, sourceY] = null;
        foreach (var capturedGamePiece in move.GamePieces)
        {
            _pieces[capturedGamePiece.X, capturedGamePiece.Y] = null;
        }

        LastMovedToX = destinationX;
        LastMovedToY = destinationY;
        if (false)
        {
            LastMoveState = EMoveState.CanContinue;
        }
        else
        {
            LastMoveState = EMoveState.Finished;
        }

        if (LastMoveState == EMoveState.Finished)
        {
            IncrementMoveCounter();
        }
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

    private void EndTurn()
    {
        IncrementMoveCounter();

        LastMoveState = EMoveState.Finished;
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
            MoveElapsedTime = MoveElapsedTime,
            GameElapsedTime = GameElapsedTime,
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