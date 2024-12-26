using Ttc.Model.Players;

namespace Ttc.Model.Teams;

/// <summary>
/// A TTC Aalst <see cref="Player"/> in a <see cref="Team"/>
/// </summary>
public class TeamPlayer
{
    #region Properties
    public int PlayerId { get; set; }
    public TeamPlayerType Type { get; set; }
    #endregion

    public override string ToString() => $"PlayerId={PlayerId}, Type={Type}";
}

public enum TeamPlayerType
{
    Standard,
    Captain,
    Reserve
}
