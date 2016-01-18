using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class ClubPloegSpeler
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key voor ClubPloeg
        public int? ClubPloegId { get; set; }

        // Foreign Key voor Speler
        public int? SpelerId { get; set; }

        public int Kapitein { get; set; }

        //[ForeignKey("ClubPloegId")]
        //public ClubPloeg Ploeg { get; set; }

        //[ForeignKey("SpelerId")]
        //public Speler Speler { get; set; }
    }
}
