using System.Text.Json.Serialization;

namespace Domain;

public class CheckersGame : AbstractDatabaseEntity
{
    public string? WhitePlayerId { get; set; }
    public string? BlackPlayerId { get; set; }

    public int CheckersRulesetId { get; set; }
    [JsonIgnore] public CheckersRuleset? CheckersRuleset { get; set; }

    [JsonIgnore] public ICollection<CheckersState>? CheckersStates { get; set; }
    [JsonIgnore] public CheckersState? CurrentCheckersState => CheckersStates?.MaxBy(c => c.CreatedAt);

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public EPlayerColor? Winner { get; set; }

    public ICollection<CheckersState> AssertSufficientCheckersStates()
    {
        if (CheckersStates == null || CheckersStates.Count < 1) throw new InsufficientCheckersStatesException(this);
        return CheckersStates!;
    }

    public override string ToString()
    {
        var result = $"CheckersGame(ID: {Id}, Started at {StartedAt}";
        result += EndedAt == null ? "" : $", Ended at {EndedAt}";
        result += ")";
        return result;
    }
}