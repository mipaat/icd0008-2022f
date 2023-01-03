using Domain.Model.Helpers;
using Timer = System.Timers.Timer;

namespace GameBrain.AI;

internal delegate float GameStateHeuristicFunc(CheckersBrain checkersBrain, EPlayerColor ePlayerColor);

internal class HeuristicMove : IComparable<HeuristicMove>
{
    private readonly GameStateHeuristicFunc _gameStateHeuristicFunc;
    private readonly EPlayerColor _playerColor;
    private readonly CheckersBrain _resultingCheckersBrain;

    public readonly Move Move;
    private List<HeuristicMove>? _children;
    private float? _gameStateHeuristic;
    private bool _nextLevelCalculated;

    private HeuristicMove(Move move, CheckersBrain resultingCheckersBrain,
        GameStateHeuristicFunc gameStateHeuristicFunc,
        EPlayerColor playerColor)
    {
        Move = move;
        _resultingCheckersBrain = resultingCheckersBrain;
        _gameStateHeuristicFunc = gameStateHeuristicFunc;
        _playerColor = playerColor;
    }

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
            if (IsEndState || _children == null || _children.Count == 0 || !_nextLevelCalculated)
                return GameStateHeuristic;

            return _playerColor == _resultingCheckersBrain[Move.ToX, Move.ToY]!.Value.Player
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

    public bool CalculateNextLevel(ref TimeoutException? timeoutException)
    {
        CheckTimeout(ref timeoutException);
        if (IsEndState) return true;
        bool finished;
        if (_children != null)
        {
            finished = true;
            foreach (var heuristicMove in _children)
            {
                CheckTimeout(ref timeoutException);
                finished &= heuristicMove.CalculateNextLevel(ref timeoutException);
            }

            _nextLevelCalculated = true;

            return finished;
        }

        CheckTimeout(ref timeoutException);
        _children = GetHeuristicMoves(_resultingCheckersBrain, _gameStateHeuristicFunc, _playerColor,
            ref timeoutException);
        finished = true;
        foreach (var heuristicMove in _children)
        {
            CheckTimeout(ref timeoutException);
            finished &= heuristicMove.IsEndState;
        }

        return finished;
    }

    public static List<HeuristicMove> GetHeuristicMoves(
        CheckersBrain checkersBrain,
        GameStateHeuristicFunc gameStateHeuristicFunc,
        EPlayerColor playerColor)
    {
        TimeoutException? _ = null;
        return GetHeuristicMoves(checkersBrain, gameStateHeuristicFunc, playerColor, ref _);
    }

    private static List<HeuristicMove> GetHeuristicMoves(
        CheckersBrain checkersBrain,
        GameStateHeuristicFunc gameStateHeuristicFunc,
        EPlayerColor playerColor,
        ref TimeoutException? timeoutException)
    {
        var result = new List<HeuristicMove>();

        var checkersGame = checkersBrain.CheckersGame;
        CheckTimeout(ref timeoutException);
        foreach (var move in checkersBrain.AvailableMoves())
        {
            CheckTimeout(ref timeoutException);
            var newCheckersBrain = new CheckersBrain(checkersGame.GetClone());
            CheckTimeout(ref timeoutException);
            newCheckersBrain.Move(move.FromX, move.FromY, move.ToX, move.ToY);
            result.Add(new HeuristicMove(move, newCheckersBrain, gameStateHeuristicFunc, playerColor));
        }

        return result;
    }

    private static void CheckTimeout(ref TimeoutException? timeoutException)
    {
        if (timeoutException != null) throw timeoutException;
    }
}

public abstract class AbstractCheckersAiMinMax : AbstractCheckersAi
{
    private static readonly Random Random = new();

    protected abstract float GetGameStateHeuristic(CheckersBrain checkersBrain, EPlayerColor playerColor);

    protected override Move? GetMove(CheckersBrain checkersBrain)
    {
        var availableMoves = checkersBrain.AvailableMoves();
        if (availableMoves.Count == 1) return availableMoves[0];

        var availableHeuristicMoves =
            HeuristicMove.GetHeuristicMoves(checkersBrain, GetGameStateHeuristic, checkersBrain.CurrentTurnPlayerColor);

        var timer = new Timer(900);
        TimeoutException? timeoutException = null;
        timer.Elapsed += (_, _) => timeoutException = new TimeoutException();

        var finished = false;
        timer.Start();
        try
        {
            while (!finished)
            {
                var depthReached = true;
                foreach (var heuristicMove in availableHeuristicMoves)
                    depthReached = depthReached && heuristicMove.CalculateNextLevel(ref timeoutException);

                finished = depthReached;
            }
        }
        catch (TimeoutException)
        {
            timer.Stop();
        }

        return availableHeuristicMoves.OrderBy(_ => Random.Next()).Max()?.Move;
    }
}