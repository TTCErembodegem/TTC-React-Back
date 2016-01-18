using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.Model
{
    [Table("spelerweek")]
    public class WeekSpeler
    {
        public int ID { get; set; }

        // Foreign Key voor Speler
        public int SpelerID { get; set; }

        [ForeignKey("SpelerID")]
        public Speler Speler { get; set; }

        public string Beschrijving { get; set; }

        public DateTime Datum { get; set; }

    }
}
