namespace Ttc.Model.Matches;

public class MatchReport
{
    #region Properties
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public string Text { get; set; }
    #endregion

    public override string ToString() => $"MatchId: {MatchId}, PlayerId: {PlayerId}, Text: {Text}";
}

public class MatchComment
{
    #region Properties
    public int Id { get; set; }
    public int MatchId { get; set; }
    public string Text { get; set; }
    public int PlayerId { get; set; }
    public DateTime PostedOn { get; set; }
    public bool Hidden { get; set; }
    public string ImageUrl { get; set; }
    #endregion

    public override string ToString() => $"MatchId: {MatchId}, Text: {Text}, PlayerId: {PlayerId}";
}
