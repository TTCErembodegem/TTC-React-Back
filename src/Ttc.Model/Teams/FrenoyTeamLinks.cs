namespace Ttc.Model.Teams;

/// <summary>
/// Team details for linking with Frenoy
/// </summary>
public class FrenoyTeamLinks
{
    #region Properties
    /// <summary>
    /// Frenoy's unique division identifier
    /// </summary>
    public int DivisionId { get; set; }

    /// <summary>
    /// Link to Frenoy team overview pages
    /// ex: 841_A = DivisionId_TeamCode
    /// </summary>
    public string LinkId { get; set; }

    /// <summary>
    /// Frenoy's unique division identifier
    /// ex: 2337-4 = DivisionId-SomeNumber(?)
    /// </summary>
    public string TeamId { get; set; }
    #endregion

    public override string ToString() => $"DivisionId={DivisionId}, LinkId={LinkId}, TeamId={TeamId}";
}
