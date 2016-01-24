using System.Collections.Generic;

namespace Ttc.Model.Teams
{
    /// <summary>
    /// One TTC Erembodegem Team in a Division/Reeks
    /// </summary>
    public class Team
    {
        public int ReeksId { get; set; }

        /// <summary>
        /// TTC Erembodegem Team code (A, B, C, ...)
        /// </summary>
        public string TeamCode { get; set; }
        public ICollection<TeamPlayer> Players { get; set; }
        public ICollection<OpposingTeam> Opponents { get; set; }

        /// <summary>
        /// Vttl or Sporta
        /// </summary>
        public string Competition { get; set; }
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

        public override string ToString()
        {
            return $"{Competition} {Year} {TeamCode}: {DivisionName}";
        }
    }
}