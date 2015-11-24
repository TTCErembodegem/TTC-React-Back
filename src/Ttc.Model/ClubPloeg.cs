using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("clubploeg")]
    public class ClubPloeg
    {
        [Key]
        public int ID { get; set; }

        // Foreign Key voor Reeks
        public int? ReeksId { get; set; }

        // Foreign Key voor Club
        public int? ClubId { get; set; }

        //[ForeignKey("ClubId")]
        //public Club Club { get; set; }

        //[ForeignKey("ReeksId")]
        //public Reeks Reeks { get; set; }

        public string Code { get; set; }
    }
}
