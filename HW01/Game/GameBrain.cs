namespace Game;

public class GameBrain : AbstractGameBrain
{
    public readonly GameOptions GameOptions;
    public override int Width => GameOptions.Width;
    public override int Height => GameOptions.Height;

    private GamePiece?[,] _pieces;

    public override bool IsSquareBlack(int x, int y)
    {
        return (x + y) % 2 != 0;
    }

    public override GamePiece? this[int x, int y] => _pieces[x, y];

    private string? _whitePlayerId;
    private string? _blackPlayerId;

    public GameBrain(GameOptions gameOptions)
    {
        CheckDimensionsValid(gameOptions.Width, gameOptions.Height);
        GameOptions = gameOptions;
        _pieces = new GamePiece?[gameOptions.Width, gameOptions.Height];
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
                    _pieces[x, y] = new GamePiece(x, y, playerColor);                    
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
        if (playerId.Equals(_whitePlayerId))
        {
            return EPlayerColor.White;
        }

        if (playerId.Equals(_blackPlayerId))
        {
            return EPlayerColor.Black;
        }

        throw new KeyNotFoundException($"Player with ID {playerId} not found in current game!");
    }

    public override bool IsMoveValid(int sourceX, int sourceY, int destinationX, int destinationY)
    {
        throw new NotImplementedException();
    }
}