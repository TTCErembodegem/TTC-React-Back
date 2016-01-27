using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model;

namespace Ttc.DataAccess.Entities
{
    internal class Verslag
    {
        [Key]
        public int Id { get; set; }
        public int KalenderId { get; set; }
        public int SpelerId { get; set; }

        [ForeignKey("KalenderId")]
        public Kalender Kalender { get; set; }

        //public ICollection<VerslagIndividueel> Individueel { get; set; }
        //public ICollection<VerslagSpeler> Spelers { get; set; }

        public string Beschrijving { get; set; }
        public int? UitslagThuis { get; set; }
        public int? UitslagUit { get; set; }
        public int WO { get; set; }
        public int? Details { get; set; }
    }
}
