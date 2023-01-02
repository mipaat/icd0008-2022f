using System.Text.Json.Serialization;
using Common;

namespace Domain;

public class CheckersGame : AbstractDatabaseEntity, ICloneable
{
    public string? WhitePlayerName { get; set; }
    public string? BlackPlayerName { get; set; }
    public EAiType? WhiteAiType { get; set; }
    public EAiType? BlackAiType { get; set; }

    public int CheckersRulesetId { get; set; }
    [ExpectedNotNull] [JsonIgnore] public CheckersRuleset? CheckersRuleset { get; set; }

    [ExpectedNotNull] [JsonIgnore] public ICollection<CheckersState>? CheckersStates { get; set; }

    [ExpectedNotNull]
    [JsonIgnore]
    public CheckersState? CurrentCheckersState => CheckersStates?.MaxBy(c => c.CreatedAt);

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public EPlayerColor? Winner { get; set; }
    public EPlayerColor? DrawProposedBy { get; set; }

    public ICollection<CheckersState> AssertSufficientCheckersStates()
    {
        if (CheckersStates == null || CheckersStates.Count < 1) throw new InsufficientCheckersStatesException(this);
        return CheckersStates!;
    }

    public DateTime LastPlayed => CurrentCheckersState?.CreatedAt ?? EndedAt ?? StartedAt;

    public bool Tied => Ended && Winner == null;
    public bool Ended => EndedAt != null;

    private Player? _whitePlayer;

    public Player WhitePlayer
    {
        get
        {
            if (_whitePlayer == null || _whitePlayer.CheckersGame != this)
                _whitePlayer = new Player(this, EPlayerColor.White);
            return _whitePlayer;
        }
    }

    private Player? _blackPlayer;

    public Player BlackPlayer
    {
        get
        {
            if (_blackPlayer == null || _blackPlayer.CheckersGame != this)
                _blackPlayer = new Player(this, EPlayerColor.Black);
            return _blackPlayer;
        }
    }

    public Player Player(EPlayerColor playerColor) => playerColor switch
    {
        EPlayerColor.Black => BlackPlayer,
        EPlayerColor.White => WhitePlayer,
        _ => throw new UnknownPlayerColorException(playerColor)
    };

    public Player? WinnerPlayer => Winner != null ? Player(Winner.Value) : null;

    public Player OtherPlayer(Player player) => player.Color switch
    {
        EPlayerColor.Black => Player(EPlayerColor.White),
        EPlayerColor.White => Player(EPlayerColor.Black),
        _ => throw new UnknownPlayerColorException(player.Color)
    };

    public EPlayerColor OtherPlayer(EPlayerColor playerColor) => OtherPlayer(Player(playerColor)).Color;

    public override string ToString()
    {
        var result = $"CheckersGame(ID: {Id}, Started at {StartedAt}";
        result += EndedAt == null ? "" : $", Ended at {EndedAt}";
        result += ")";
        return result;
    }

    public CheckersGame GetClone()
    {
        return new CheckersGame
        {
            WhitePlayerName = WhitePlayerName,
            BlackPlayerName = BlackPlayerName,
            WhiteAiType = WhiteAiType,
            BlackAiType = BlackAiType,
            CheckersRulesetId = CheckersRulesetId,
            CheckersRuleset = CheckersRuleset,
            CheckersStates = CheckersStates?.ToList(),
            StartedAt = StartedAt,
            EndedAt = EndedAt,
            Winner = Winner,
            _blackPlayer = null,
            _whitePlayer = null
        };
    }

    public object Clone()
    {
        return GetClone();
    }
}