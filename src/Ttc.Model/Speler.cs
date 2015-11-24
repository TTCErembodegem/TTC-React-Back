using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("speler")]
    public class Speler
    {
        [Key]
        public int ID { get; set; }

        public string Naam { get; set; }

        public string LinkKaartVTTL { get; set; }

        public string KlassementVTTL { get; set; }

        public string KlassementSporta { get; set; }

        public string Stijl { get; set; }

        public string BesteSlag { get; set; }

        public int? ComputerNummerVTTL { get; set; }

        public string Adres { get; set; }

        public string Gemeente { get; set; }

        public string GSM { get; set; }

        public string Email { get; set; }

        public string Paswoord { get; set; }

        public int? ClubIdVTTL { get; set; }

        public int? ClubIdSporta { get; set; }

        public string NaamKort { get; set; }

        public int? VolgnummerVTTL { get; set; }

        public int? IndexVTTL { get; set; }

        public int? LidNummerSporta { get; set; }

        public int? VolgnummerSporta { get; set; }

        public int? IndexSporta { get; set; }

        [Column("Gestopt")]
        public int? JaarGestopt { get; set; }

        public int? Toegang { get; set; }

        public string LinkKaartSporta { get; set; }

        public bool IsGestopt { get { return JaarGestopt != null; } }
    }
}
