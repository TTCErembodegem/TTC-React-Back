using Ttc.Model.Players;

namespace Ttc.Model.Teams
{
    /// <summary>
    /// A TTC Erembodegem <see cref="Player"/> in a <see cref="Team"/>
    /// </summary>
    public class TeamPlayer
    {
        public int PlayerId { get; set; }
        public TeamPlayerType Type { get; set; }

        public override string ToString() => $"PlayerId={PlayerId}, Type={Type}";
    }

    public enum TeamPlayerType
    {
        Standard,
        Captain,
        Reserve
    }
}