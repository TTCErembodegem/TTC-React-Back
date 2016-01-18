using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class Klassement
    {
        [Key]
        public string Code { get; set; }

        public int WaardeVttl { get; set; }

        public int WaardeSporta { get; set; }
    }
}
