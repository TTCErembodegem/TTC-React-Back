using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace FrenoySyncer
{
    class Program
    {
        //const string FrenoyVttlWsdlUrl = "http://api.vttl.be/0.7/?wsdl";
        //const string FrenoySportaWsdlUrl = "http://tafeltennis.sporcrea.be/api/?wsdl";
        // Export: reeks, clubploeg, clubploegspeler and kalender

        #region Configuration
        private static FrenoySyncOptions VttlOptions => new FrenoySyncOptions
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

        private static FrenoySyncOptions SportaOptions => new FrenoySyncOptions
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
                ["D"] = new[] { "Leo", "Guy", "Patrick", "Tuur", "Peter V" },
                ["E"] = new[] { "Dirk K.", "Etienne", "Peter N", "Marnix", "Thierry", "Martin" },
                ["F"] = new[] { "Tim", "Rudi", "Daniel", "Tim", "Etienne C.", "Myriam", "Wim", "Martin" }
            }
        };
        #endregion

        static void Main(string[] args)
        {
            using (var vttl = new FrenoySync(VttlOptions))
            {
                vttl.Sync();
                vttl.WriteLog();
            }

            //using (var sporta = new FrenoySync(SportaOptions, false))
            //{
            //    sporta.Sync();
            //    sporta.WriteLog();
            //}

            //Console.WriteLine("All done");
            //Console.ReadLine();
        }
    }
}