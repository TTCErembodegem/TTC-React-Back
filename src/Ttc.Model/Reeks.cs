using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("reeks")]
    public class Reeks
    {
        [Key]
        public int ID { get; set; }

        public string Competitie { get; set; }

        [Column("Reeks")]
        public string ReeksNummer { get; set; }

        public string ReeksType { get; set; }

        public string ReeksCode { get; set; }

        public int? Jaar { get; set; }

        public string LinkID { get; set; }

        public string FrenoyTeamId { get; set; }
        public int FrenoyDivisionId { get; set; }
    }
}
