namespace Ttc.Model.Matches;

public class MatchScore
{
    #region Properties
    public int Home { get; set; }
    public int Out { get; set; }
    #endregion

    #region Constructors
    public MatchScore()
    {
    }

    public MatchScore(int homeScore, int outScore)
    {
        Home = homeScore;
        Out = outScore;
    }
    #endregion

    public override string ToString() => $"{Home}-{Out}";
}

public enum MatchOutcome
{
    NotYetPlayed,
    Won,
    Lost,
    Draw,
    WalkOver,
    BeingPlayed
}
