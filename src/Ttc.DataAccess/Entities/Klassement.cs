using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    [Table("klassement")]
    internal class Klassement
    {
        [Key]
        public string Code { get; set; }

        public int WaardeVTTL { get; set; }

        public int WaardeSporta { get; set; }
    }
}
