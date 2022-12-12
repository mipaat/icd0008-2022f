using Domain;
using Timer = System.Timers.Timer;

namespace GameBrain.AI;

internal delegate float GameStateHeuristicFunc(CheckersBrain checkersBrain, EPlayerColor ePlayerColor);

internal class HeuristicMove : IComparable<HeuristicMove>
{
    private HeuristicMove(Move move, CheckersBrain resultingCheckersBrain, GameStateHeuristicFunc gameStateHeuristicFunc,
        EPlayerColor playerColor)
    {
        Move = move;
        _resultingCheckersBrain = resultingCheckersBrain;
        _gameStateHeuristicFunc = gameStateHeuristicFunc;
        _playerColor = playerColor;
    }

    public readonly Move Move;
    private readonly CheckersBrain _resultingCheckersBrain;
    private List<HeuristicMove>? _children;
    private readonly EPlayerColor _playerColor;

    private readonly GameStateHeuristicFunc _gameStateHeuristicFunc;
    private float? _gameStateHeuristic;

    private float GameStateHeuristic
    {
        get
        {
            _gameStateHeuristic ??= _gameStateHeuristicFunc(_resultingCheckersBrain, _playerColor);
            return _gameStateHeuristic.Value;
        }
    }

    private bool IsEndState => _resultingCheckersBrain.Ended;

    private float Heuristic
    {
        get
        {
            if (IsEndState || _children == null || _children.Count == 0) return GameStateHeuristic;
            return _playerColor == _resultingCheckersBrain.CurrentTurnPlayerColor
                ? _children!.Max(move => move.Heuristic)
                : _children!.Min(move => move.Heuristic);
        }
    }

    public int CompareTo(HeuristicMove? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Heuristic.CompareTo(other.Heuristic);
    }

    public bool CalculateNextLevel(TimeoutException? timeoutException)
    {
        if (IsEndState) return true;
        bool finished;
        if (_children != null)
        {
            finished = true;
            foreach (var heuristicMove in _children)
            {
                if (timeoutException != null) throw timeoutException;
                finished &= heuristicMove.CalculateNextLevel(timeoutException);
            }

            return finished;
        }

        _children = GetHeuristicMoves(_resultingCheckersBrain, _gameStateHeuristicFunc, _playerColor);
        finished = true;
        foreach (var heuristicMove in _children)
        {
            finished &= heuristicMove.IsEndState;
        }

        return finished;
    }

    public static List<HeuristicMove> GetHeuristicMoves(CheckersBrain checkersBrain, GameStateHeuristicFunc gameStateHeuristicFunc,
        EPlayerColor playerColor)
    {
        var result = new List<HeuristicMove>();

        var checkersGame = checkersBrain.GetSaveGameState();
        var checkersState = checkersGame.CurrentCheckersState;
        foreach (var move in checkersBrain.AvailableMoves())
        {
            var newCheckersBrain = new CheckersBrain(checkersGame.GetClone());
            newCheckersBrain.Move(move.FromX, move.FromY, move.ToX, move.ToY);
            result.Add(new HeuristicMove(move, newCheckersBrain, gameStateHeuristicFunc, playerColor));
        }
        if (checkersState != null) checkersGame.CheckersStates!.Remove(checkersState);
        return result;
    }
}

public abstract class AbstractCheckersAiMinMax : AbstractCheckersAi
{
    private static readonly Random Random = new();

    protected abstract float GetGameStateHeuristic(CheckersBrain checkersBrain, EPlayerColor playerColor);

    protected override Move? GetMove(CheckersBrain checkersBrain)
    {
        var availableMoves =
            HeuristicMove.GetHeuristicMoves(checkersBrain, GetGameStateHeuristic, checkersBrain.CurrentTurnPlayerColor);

        var timer = new Timer(500);
        TimeoutException? timeoutException = null;
        timer.Elapsed += (_, _) => timeoutException = new TimeoutException();

        var finished = false;
        timer.Start();
        try
        {
            while (!finished)
            {
                var depthReached = true;
                foreach (var heuristicMove in availableMoves)
                {
                    depthReached = depthReached && heuristicMove.CalculateNextLevel(timeoutException);
                }

                finished = depthReached;
            }
        }
        catch (TimeoutException)
        {
            timer.Stop();
        }

        return availableMoves.OrderBy(_ => Random.Next()).Max()?.Move;
    }
}