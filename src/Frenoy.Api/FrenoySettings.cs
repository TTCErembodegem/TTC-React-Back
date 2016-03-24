using System.Collections.Generic;
using Frenoy.Api.FrenoyVttl;
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

        public override string ToString() => $"FrenoyClub={FrenoyClub}, FrenoySeason={FrenoySeason}, Competitie={Competition}, ReeksType={DivisionType}, Jaar={Year}";

        /// <summary>
        /// Keys = TeamCode
        /// Values = Players with first player = Captain
        /// </summary>
        public Dictionary<string, string[]> Players { get; set; }

        public static FrenoySettings VttlSettings => new FrenoySettings
        {
            FrenoyClub = "OVL134",
            FrenoySeason = 16,
            Year = 2015,
            Competition = Competition.Vttl,
            DivisionType = "Prov",
            Players = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "Dirk DS.", "Kharmis", "Jorn", "Sami", "Jurgen E.", "Wouter" },
                ["B"] = new[] { "Bart", "Gerdo", "Jens", "Dimitri", "Patrick", "Geert" },
                ["C"] = new[] { "Thomas", "Dirk B", "Jelle", "Arne", "Laurens", "Hugo" },
                ["D"] = new[] { "Jan", "Marc", "Luc", "Maarten", "Veerle", "Patrick DS" },
                ["E"] = new[] { "Dirk K.", "Leo", "Dries", "Guy", "Peter N", "Tuur", "Peter V" },
                ["F"] = new[] { "Tim", "Etienne", "Thierry", "Rudi", "Marnix", "Daniel", "Wim" }
            }
        };

        public static FrenoySettings SportaSettings => new FrenoySettings
        {
            FrenoyClub = "4055",
            FrenoySeason = 16,
            Year = 2015,
            Competition = Competition.Sporta,
            DivisionType = "Afd",
            Players = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "Dirk DS.", "Kharmis", "Jorn", "Sami", "Wouter" },
                ["B"] = new[] { "Bart", "Patrick", "Geert", "Dirk B", "Jelle" },
                ["C"] = new[] { "Dries", "Maarten", "Luc", "Jan", "Veerle" },
                ["D"] = new[] { "Leo", "Guy", "Patrick DS", "Tuur", "Peter V" },
                ["E"] = new[] { "Dirk K.", "Etienne", "Peter N", "Marnix", "Thierry" },
                ["F"] = new[] { "Tim", "Rudi", "Daniel", "Etienne C.", "Myriam", "Wim" }
            }
        };
    }
}