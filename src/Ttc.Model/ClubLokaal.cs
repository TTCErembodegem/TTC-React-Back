using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("clublokaal")]
    public class ClubLokaal
    {
        [Key]
        public int ID { get; set; }

        // Foreign Key voor Club
        public int ClubId { get; set; }

        //[ForeignKey("ClubId")]
        //public Club Club { get; set; }

        public string Lokaal { get; set; }

        public string Adres { get; set; }

        public string Gemeente { get; set; }

        public int? Hoofd { get; set; }

        public int? Postcode { get; set; }

        public string Telefoon { get; set; }
    }
}
