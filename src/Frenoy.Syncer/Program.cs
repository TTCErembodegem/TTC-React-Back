using System.Collections.Generic;
using Frenoy.Api;
using Ttc.DataAccess;
using Ttc.Model.Players;

namespace Frenoy.Syncer
{
    class Program
    {
        //const string FrenoyVttlWsdlUrl = "http://api.vttl.be/0.7/?wsdl";
        //const string FrenoySportaWsdlUrl = "http://tafeltennis.sporcrea.be/api/?wsdl";
        // Export: reeks, clubploeg, clubploegspeler and kalender

        static void Main(string[] args)
        {
            using (var dbContext = new TtcDbContext())
            {
                var vttl = new FrenoyApi(dbContext, Competition.Vttl);
                vttl.SyncAll();

                var sporta = new FrenoyApi(dbContext, Competition.Sporta);
                //sporta.SyncClubLokalen();
                //sporta.SyncAll();
            }

            //Console.WriteLine("All done");
            //Console.ReadLine();
        }
    }
}