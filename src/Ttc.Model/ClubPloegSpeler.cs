using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("clubploegspeler")]
    public class ClubPloegSpeler
    {
        [Key]
        public int ID { get; set; }

        // Foreign Key voor ClubPloeg
        public int? ClubPloegID { get; set; }

        // Foreign Key voor Speler
        public int? SpelerID { get; set; }

        public int Kapitein { get; set; }

        //[ForeignKey("ClubPloegID")]
        //public ClubPloeg Ploeg { get; set; }

        //[ForeignKey("SpelerID")]
        //public Speler Speler { get; set; }
    }
}
