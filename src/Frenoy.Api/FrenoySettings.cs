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
        public int FrenoySeason => Year - 2000 + 1;
        public Competition Competition { get; set; }
        public string DivisionType { get; set; }
        public int Year { get; set; }

        public override string ToString() => $"FrenoyClub={FrenoyClub}, FrenoySeason={FrenoySeason}, Competitie={Competition}, Division={DivisionType}, Jaar={Year}";

        public static FrenoySettings VttlSettings(int currentSeason)
        {
            return new FrenoySettings
            {
                FrenoyClub = "OVL134",
                Year = currentSeason,
                Competition = Competition.Vttl,
                DivisionType = "Prov",
            };
        }

        public static FrenoySettings SportaSettings(int currentSeason)
        {
            return new FrenoySettings
            {
                FrenoyClub = "4055",
                Year = currentSeason,
                Competition = Competition.Sporta,
                DivisionType = "Afd",
            };
        }
    }
}