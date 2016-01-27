using System.Collections.Generic;

namespace FrenoySyncer
{
    /// <summary>
    /// Frenoy GetClubTeams => The Teams within a Club. Each ClubTeam plays in a Division
    /// Frenoy Divisions => Prov 3C
    /// </summary>
    public class FrenoySyncOptions
    {
        public string FrenoyClub { get; set; }
        public string FrenoySeason { get; set; }
        public string Competitie { get; set; }
        public string ReeksType { get; set; }
        public int Jaar { get; set; }

        /// <summary>
        /// Keys = TeamCode
        /// Values = Players with first player = Captain
        /// </summary>
        public Dictionary<string, string[]> Players { get; set; }
    }
}