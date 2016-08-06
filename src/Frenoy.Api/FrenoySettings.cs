using System.Collections.Generic;
using Frenoy.Api.FrenoyVttl;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Frenoy.Api
{
    /// <summary>
    /// Frenoy GetClubTeams => The Teams within a Club. Each ClubTeam plays in a Division
    /// Frenoy Divisions => Prov 3C
    /// </summary>
    public class FrenoySettings
    {
        public string FrenoyClub { get; set; }
        public int FrenoySeason { get; set; }
        public Competition Competition { get; set; }
        public string DivisionType { get; set; }
        public int Year { get; set; }

        public override string ToString() => $"FrenoyClub={FrenoyClub}, FrenoySeason={FrenoySeason}, Competitie={Competition}, Division={DivisionType}, Jaar={Year}";

        public static FrenoySettings VttlSettings => new FrenoySettings
        {
            FrenoyClub = "OVL134",
            FrenoySeason = Constants.FrenoySeason,
            Year = Constants.CurrentSeason,
            Competition = Competition.Vttl,
            DivisionType = "Prov",
        };

        public static FrenoySettings SportaSettings => new FrenoySettings
        {
            FrenoyClub = "4055",
            FrenoySeason = Constants.FrenoySeason,
            Year = Constants.CurrentSeason,
            Competition = Competition.Sporta,
            DivisionType = "Afd",
        };
    }
}