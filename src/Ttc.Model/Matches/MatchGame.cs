namespace Ttc.Model.Matches;

public class MatchGame
{
    #region Properties
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int MatchNumber { get; set; }
    public int HomePlayerUniqueIndex { get; set; }
    public int OutPlayerUniqueIndex { get; set; }
    public int HomePlayerSets { get; set; }
    public int OutPlayerSets { get; set; }
    public MatchOutcome Outcome { get; set; }
    #endregion

    public override string ToString() => $"{MatchNumber}, Home: {HomePlayerUniqueIndex}={HomePlayerSets}, Out: {OutPlayerUniqueIndex}={OutPlayerSets}, Outcome={Outcome}";
}
