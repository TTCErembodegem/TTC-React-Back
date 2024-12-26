namespace Ttc.Model.Teams;

public class OpposingTeam
{
    #region Properties
    /// <summary>
    /// Team A, B, C, ...
    /// </summary>
    public string TeamCode { get; set; }

    public int ClubId { get; set; }
    #endregion

    public OpposingTeam(int clubId, string teamCode)
    {
        TeamCode = teamCode;
        ClubId = clubId;
    }

    public OpposingTeam()
    {

    }

    public static OpposingTeam Create(int? clubId, string teamCode)
    {
        if (!clubId.HasValue) return null;
        return new OpposingTeam(clubId.Value, teamCode);
    }

    public override string ToString() => $"ClubId={ClubId}, Team={TeamCode}";
}
