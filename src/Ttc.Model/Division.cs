using System.Collections.Generic;

namespace Ttc.Model
{
    public class Division
    {
        /// <summary>
        /// Our Reeks.Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// TTC Erembodegem Team code (A, B, C, ...)
        /// </summary>
        public string TeamCode { get; set; }
        public ICollection<Team> Opponents { get; set; }

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
        /// Links to Frenoy website and API
        /// </summary>
        public FrenoyTeam Frenoy { get; set; }

        public override string ToString()
        {
            return $"{Competition} {Year}: {DivisionName}";
        }
    }
}