using System.Collections.Generic;
using Frenoy.Api;
using Ttc.DataAccess;
using Ttc.DataAccess.Migrations;
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
            using (var context = new TtcDbContext())
            {
                Configuration.Seed(context, true, true);
                context.SaveChanges();
            }

            //Console.WriteLine("All done");
            //Console.ReadLine();
        }
    }
}