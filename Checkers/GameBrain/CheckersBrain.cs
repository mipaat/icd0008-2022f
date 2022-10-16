using Domain;

namespace GameBrain;

public class CheckersBrain : AbstractCheckersBrain
{
    private readonly CheckersOptions _checkersOptions;
    private readonly CheckersGame _checkersGame;
    public override int Width => _checkersOptions.Width;
    public override int Height => _checkersOptions.Height;

    private readonly GamePiece?[,] _pieces;

    public override bool IsSquareBlack(int x, int y)
    {
        return (x + y) % 2 != 0;
    }

    public override GamePiece? this[int x, int y] => _pieces[x, y];

    private string? WhitePlayerId => _checkersGame.WhitePlayerId;
    private string? BlackPlayerId => _checkersGame.BlackPlayerId;

    public CheckersBrain(CheckersGame checkersGame)
    {
        _checkersGame = checkersGame;

        var checkersOptions = checkersGame.CheckersOptions;
        CheckDimensionsValid(checkersOptions.Width, checkersOptions.Height);
        _checkersOptions = checkersOptions;

        if (_checkersGame.CurrentCheckersState != null)
        {
            var currentCheckersState = _checkersGame.CurrentCheckersState;
            _pieces = currentCheckersState.GamePieces;
            WhiteMoves = currentCheckersState.WhiteMoves;
            BlackMoves = currentCheckersState.BlackMoves;
            GameElapsedTime = currentCheckersState.GameElapsedTime;
            MoveElapsedTime = currentCheckersState.MoveElapsedTime;
        }
        else
        {
            _pieces = new GamePiece?[checkersOptions.Width, checkersOptions.Height];
            InitializePieces();
        }
    }

    public CheckersBrain(CheckersOptions checkersOptions)
    {
        _checkersGame = new CheckersGame
        {
            WhitePlayerId = null, // TODO: figure out how Player IDs are actually going to work. Then implement.
            BlackPlayerId = null,
            CheckersOptions = checkersOptions,
            CheckersStates = new List<CheckersState>(),
            StartedAt = DateTime.Now
        };

        CheckDimensionsValid(checkersOptions.Width, checkersOptions.Height);
        _checkersOptions = checkersOptions;
        _pieces = new GamePiece?[checkersOptions.Width, checkersOptions.Height];
        InitializePieces();
    }

    private static void CheckDimensionsValid(int x, int y)
    {
        if (RowsPerPlayer(y) < 1 || x < 4)
        {
            throw new ArgumentOutOfRangeException($"Dimensions ({x}, {y}) too small to initialize GameBoard!");
        }
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

    public override EPlayerColor PlayerColor(string playerId)
    {
        if (playerId.Equals(WhitePlayerId))
        {
            return EPlayerColor.White;
        }

        if (playerId.Equals(BlackPlayerId))
        {
            return EPlayerColor.Black;
        }

        throw new KeyNotFoundException($"Player with ID {playerId} not found in current game!");
    }

    public override DateTime StartedAt { get; set; }
    public override DateTime? EndedAt { get; set; }
    public sealed override TimeSpan MoveElapsedTime { get; set; }
    public sealed override TimeSpan GameElapsedTime { get; set; }
    public sealed override int WhiteMoves { get; set; }
    public sealed override int BlackMoves { get; set; }

    public override bool IsMoveValid(int sourceX, int sourceY, int destinationX, int destinationY)
    {
        return true; //TODO: Actually implement!
    }

    public override EMoveState Move(int sourceX, int sourceY, int destinationX, int destinationY)
    {
        _pieces[destinationX, destinationY] = _pieces[sourceX, sourceY];
        _pieces[sourceX, sourceY] = null;
        return EMoveState.Finished; //TODO: Return actual move state!
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
            CreatedAt = DateTime.Now,
            GamePieces = gamePiecesCopy,
            MoveElapsedTime = MoveElapsedTime,
            GameElapsedTime = GameElapsedTime,
            WhiteMoves = WhiteMoves,
            BlackMoves = BlackMoves
        };
    }

    public override CheckersGame GetSaveGameState()
    {
        _checkersGame.CheckersStates.Add(GetCheckersState());

        return new CheckersGame
        {
            Id = _checkersGame.Id,
            WhitePlayerId = WhitePlayerId,
            BlackPlayerId = BlackPlayerId,
            CheckersOptions = _checkersOptions,
            CheckersStates = new List<CheckersState>(_checkersGame.CheckersStates),
            StartedAt = _checkersGame.StartedAt,
            EndedAt = _checkersGame.EndedAt,
            Winner = _checkersGame.Winner
        };
    }
}