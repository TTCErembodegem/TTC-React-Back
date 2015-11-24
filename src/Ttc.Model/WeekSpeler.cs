using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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
