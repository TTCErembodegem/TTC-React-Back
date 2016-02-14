using System.Collections.Generic;
using Frenoy.Api;

namespace Frenoy.Syncer
{
    class Program
    {
        //const string FrenoyVttlWsdlUrl = "http://api.vttl.be/0.7/?wsdl";
        //const string FrenoySportaWsdlUrl = "http://tafeltennis.sporcrea.be/api/?wsdl";
        // Export: reeks, clubploeg, clubploegspeler and kalender

        static void Main(string[] args)
        {
            using (var vttl = new FrenoySync(FrenoySettings.VttlSettings))
            {
                vttl.Sync();
            }

            //using (var sporta = new FrenoySync(FrenoyOptions.SportaOptions, false))
            //{
            //    sporta.Sync();
            //}

            //Console.WriteLine("All done");
            //Console.ReadLine();
        }
    }
}