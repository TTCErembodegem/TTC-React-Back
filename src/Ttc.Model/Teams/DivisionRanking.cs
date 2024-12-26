namespace Ttc.Model.Teams;

/// <summary>
/// This is mapped from Frenoy API call
/// </summary>
public class DivisionRanking
{
    public int Position { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GamesDraw { get; set; }
    public int Points { get; set; }
    public int ClubId { get; set; }
    public string TeamCode { get; set; }
    public bool IsForfait { get; set; }
}
