using System.Text.Json.Serialization;

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
    [ExpectedNotNull] [JsonIgnore] public CheckersState? CurrentCheckersState => CheckersStates?.MaxBy(c => c.CreatedAt);

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public EPlayerColor? Winner { get; set; }
    public EPlayerColor? DrawProposedBy { get; set; }

    public ICollection<CheckersState> AssertSufficientCheckersStates()
    {
        if (CheckersStates == null || CheckersStates.Count < 1) throw new InsufficientCheckersStatesException(this);
        return CheckersStates!;
    }

    public bool Tied => Ended && Winner == null;
    public bool Ended => EndedAt != null;

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
            Winner = Winner
        };
    }

    public object Clone()
    {
        return GetClone();
    }
}