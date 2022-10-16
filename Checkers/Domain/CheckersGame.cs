using System.Text.Json.Serialization;

namespace Domain;

public class CheckersGame : AbstractDatabaseEntity
{
    public string? WhitePlayerId { get; set; }
    public string? BlackPlayerId { get; set; }

    public int CheckersOptionsId { get; set; }
    [JsonIgnore] public CheckersOptions CheckersOptions { get; set; } = default!;

    [JsonIgnore] public ICollection<CheckersState> CheckersStates { get; set; } = default!;
    [JsonIgnore] public CheckersState? CurrentCheckersState => CheckersStates.MaxBy(c => c.CreatedAt);

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public EPlayerColor? Winner { get; set; }
}