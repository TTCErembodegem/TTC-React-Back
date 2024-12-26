namespace Ttc.Model.Matches;

public class MatchPlayer
{
    #region Properties
    public int Id { get; set; }
    public int MatchId { get; set; }
    public string Status { get; set; }
    public string StatusNote { get; set; }

    public int Position { get; set; }
    public string Name { get; set; }
    public string Ranking { get; set; }
    public int UniqueIndex { get; set; }
    public int Won { get; set; }

    /// <summary>
    /// True == TTC Aalst player
    /// </summary>
    public bool Home { get; set; }
    public int PlayerId { get; set; }
    public string Alias { get; set; }
    #endregion

    public override string ToString() => $"MatchId={MatchId}, Ply={Position} {Name} ({Ranking}), Won={Won}, Status={Status}";
}
