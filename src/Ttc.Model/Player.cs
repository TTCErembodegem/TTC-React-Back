using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.Model
{
    public class Player
    {
        public int Id { get; set; }

        public string Naam { get; set; }

        public string LinkKaartVttl { get; set; }

        public string KlassementVttl { get; set; }

        public string KlassementSporta { get; set; }

        public string Stijl { get; set; }

        public string BesteSlag { get; set; }

        public int? ComputerNummerVttl { get; set; }

        public string Adres { get; set; }

        public string Gemeente { get; set; }

        public string Gsm { get; set; }

        public string Email { get; set; }

        public string Paswoord { get; set; }

        public int? ClubIdVttl { get; set; }

        public int? ClubIdSporta { get; set; }

        public string NaamKort { get; set; }

        public int? VolgnummerVttl { get; set; }

        public int? IndexVttl { get; set; }

        public int? LIdNummerSporta { get; set; }

        public int? VolgnummerSporta { get; set; }

        public int? IndexSporta { get; set; }

        public int? Gestopt { get; set; }

        public int? Toegang { get; set; }

        public string LinkKaartSporta { get; set; }

        public bool IsGestopt => Gestopt != null;
    }
}
