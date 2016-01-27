using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class VerslagSpeler
    {
        [Key]
        public int Id { get; set; }
        public int VerslagId { get; set; }
        public int? SpelerId { get; set; }

        [ForeignKey("VerslagId")]
        public Verslag Verslag { get; set; }

        [ForeignKey("SpelerId")]
        public Speler Speler { get; set; }

        public int? Winst { get; set; }
        public string SpelerNaam { get; set; }
        public int? Thuis { get; set; }
        public string Klassement { get; set; }
    }
}
