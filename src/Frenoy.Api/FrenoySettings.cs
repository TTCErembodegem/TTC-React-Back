using System.Collections.Generic;
using Frenoy.Api.FrenoyVttl;

namespace Frenoy.Api
{
    /// <summary>
    /// Frenoy GetClubTeams => The Teams within a Club. Each ClubTeam plays in a Division
    /// Frenoy Divisions => Prov 3C
    /// </summary>
    public class FrenoySettings
    {
        public string FrenoyClub { get; set; }
        public string FrenoySeason { get; set; }
        public string Competitie { get; set; }
        public string ReeksType { get; set; }
        public int Jaar { get; set; }

        public override string ToString() => $"FrenoyClub={FrenoyClub}, FrenoySeason={FrenoySeason}, Competitie={Competitie}, ReeksType={ReeksType}, Jaar={Jaar}";

        /// <summary>
        /// Keys = TeamCode
        /// Values = Players with first player = Captain
        /// </summary>
        public Dictionary<string, string[]> Players { get; set; }

        public static FrenoySettings VttlSettings => new FrenoySettings
        {
            FrenoyClub = "OVL134",
            FrenoySeason = "16",
            Jaar = 2015,
            Competitie = "VTTL",
            ReeksType = "Prov",
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
            FrenoySeason = "16",
            Jaar = 2015,
            Competitie = "Sporta",
            ReeksType = "Afd",
            Players = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "Dirk DS.", "Kharmis", "Jorn", "Sami", "Wouter" },
                ["B"] = new[] { "Bart", "Patrick", "Geert", "Dirk B", "Jelle" },
                ["C"] = new[] { "Dries", "Maarten", "Luc", "Jan", "Veerle" },
                ["D"] = new[] { "Leo", "Guy", "Patrick DS", "Tuur", "Peter V" },
                ["E"] = new[] { "Dirk K.", "Etienne", "Peter N", "Marnix", "Thierry", "Martin" },
                ["F"] = new[] { "Tim", "Rudi", "Daniel", "Tim", "Etienne C.", "Myriam", "Wim", "Martin" }
            }
        };
    }
}