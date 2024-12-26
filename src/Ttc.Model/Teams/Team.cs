using Ttc.Model.Players;

namespace Ttc.Model.Teams;

/// <summary>
/// One TTC Aalst Team in a Division/Reeks
/// </summary>
public class Team
{
    #region Properties
    public int Id { get; set; }

    /// <summary>
    /// TTC Aalst Team code (A, B, C, ...)
    /// </summary>
    public string TeamCode { get; set; }
    public int ClubId { get; set; }
    public ICollection<TeamPlayer> Players { get; set; }

    // TODO: Opponents can be replaced with the Ranking
    public ICollection<OpposingTeam> Opponents { get; set; }

    /// <summary>
    /// Vttl or Sporta
    /// </summary>
    public Competition Competition { get; set; }
    public int Year { get; set; }

    /// <summary>
    /// Vttl: 2A
    /// Sporta: 1
    /// </summary>
    public string DivisionName { get; set; }

    /// <summary>
    /// Links to Frenoy website and API details of TTC Aalst Team
    /// </summary>
    public FrenoyTeamLinks Frenoy { get; set; }

    public ICollection<DivisionRanking> Ranking { get; set; }
    #endregion

    #region Constructor
    public Team()
    {
        Ranking = new List<DivisionRanking>();
    }
    #endregion

    public override string ToString() => $"{Competition} {Year} {TeamCode}: {DivisionName}";
}
