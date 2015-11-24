using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Ttc.Model
{
    [Table("klassement")]
    public class Klassement
    {
        [Key]
        public string Code { get; set; }

        public int WaardeVTTL { get; set; }

        public int WaardeSporta { get; set; }
    }
}
