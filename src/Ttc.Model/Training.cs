using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("training")]
    public class Training
    {
        [Key]
        public int ID { get; set; }

        // Foreign Key voor Kalender
        public int KalenderID { get; set; }

        // Foreign Key voor Speler
        public int SpelerID { get; set; }

        //[ForeignKey("KalenderID")]
        //public Match Kalender { get; set; }

        //[ForeignKey("SpelerID")]
        //public Speler Speler { get; set; }

        public int? Uur { get; set; }
    }
}
