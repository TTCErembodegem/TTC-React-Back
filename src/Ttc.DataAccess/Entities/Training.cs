using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class Training
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key voor Kalender
        public int KalenderId { get; set; }

        // Foreign Key voor Speler
        public int SpelerId { get; set; }

        //[ForeignKey("KalenderId")]
        //public Match Kalender { get; set; }

        //[ForeignKey("SpelerId")]
        //public Speler Speler { get; set; }

        public int? Uur { get; set; }
    }
}
