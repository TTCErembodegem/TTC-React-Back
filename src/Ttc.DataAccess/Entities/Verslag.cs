using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class Verslag
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key voor Kalender
        public int KalenderId { get; set; }

        // Foreign Key voor Speler
        public int SpelerId { get; set; }

        //[ForeignKey("KalenderId")]
        //public Match Kalender { get; set; }

        //[ForeignKey("SpelerId")]
        //public Speler Speler { get; set; }

        public string Beschrijving { get; set; }

        public int? UitslagThuis { get; set; }

        public int? UitslagUit { get; set; }

        public string Uitslag
        {
            get
            {
                return String.Format("{0} - {1}", UitslagThuis, UitslagUit);
            }
            set { }
        }

        public int? WO { get; set; }

        public int? Details { get; set; }
    }
}
