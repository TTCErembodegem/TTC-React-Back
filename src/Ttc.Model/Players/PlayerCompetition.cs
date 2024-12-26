namespace Ttc.Model.Players;

/// <summary>
/// Details for VTTL or Sporta competition
/// </summary>
public class PlayerCompetition
{
    #region Properties
    public Competition Competition { get; set; }
    public int ClubId { get; set; }
    public string FrenoyLink { get; set; }
    public string Ranking { get; set; }
    public string NextRanking { get; set; }

    /// <summary>
    /// Index in de club (volgnummer)
    /// </summary>
    public int Position { get; set; }
    
    /// <summary>
    /// ComputerNummer (VTTL) of LidNummer (Sporta)
    /// </summary>
    public int UniqueIndex { get; set; }

    /// <summary>
    /// 'Index Ranking'-Position in de club
    /// </summary>
    public int RankingIndex { get; set; }

    /// <summary>
    /// Waarde van de ranking
    /// </summary>
    public int RankingValue { get; set; }
    #endregion

    #region Constructors
    public PlayerCompetition()
    {
        
    }

    public PlayerCompetition(Competition competition, int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex, int rankingValue, string prevRanking)
    {
        Competition = competition;
        ClubId = clubId;
        FrenoyLink = frenoyLink;
        Ranking = ranking;
        Position = position;
        UniqueIndex = uniqueIndex;
        RankingIndex = rankingIndex;
        RankingValue = rankingValue;
        NextRanking = prevRanking;
    }
    #endregion

    public override string ToString() => $"Competition={Competition}, ClubId={ClubId}, Ranking={Ranking}, UniqueIndex={UniqueIndex}";
}

public enum Competition
{
    Vttl,
    Sporta
}
