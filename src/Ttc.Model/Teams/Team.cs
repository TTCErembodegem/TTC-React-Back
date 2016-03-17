using System.Collections.Generic;
using Ttc.Model.Players;

namespace Ttc.Model.Teams
{
    /// <summary>
    /// One TTC Erembodegem Team in a Division/Reeks
    /// </summary>
    public class Team
    {
        public int Id { get; set; }

        /// <summary>
        /// TTC Erembodegem Team code (A, B, C, ...)
        /// </summary>
        public string TeamCode { get; set; }
        public int ClubId { get; set; }
        public ICollection<TeamPlayer> Players { get; set; }

        // TODO: Opponents can be replaced with the Ranking
        public ICollection<OpposingTeam> Opponents { get; set; }

        public Team()
        {
            Ranking = new List<DivisionRanking>();
        }

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
        /// Links to Frenoy website and API details of TTC Erembodegem Team
        /// </summary>
        public FrenoyTeamLinks Frenoy { get; set; }

        public ICollection<DivisionRanking> Ranking { get; set; }

        public override string ToString() => $"{Competition} {Year} {TeamCode}: {DivisionName}";
    }
}