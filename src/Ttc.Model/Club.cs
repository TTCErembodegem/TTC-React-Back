using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("club")]
    public class Club
    {
        [Key]
        public int ID { get; set; }

        public string Naam { get; set; }

        public string CodeVTTL { get; set; }

        public int? Actief { get; set; }

        public int? Douche { get; set; }

        public string Website { get; set; }

        public string CodeSporta { get; set; }
    }
}
