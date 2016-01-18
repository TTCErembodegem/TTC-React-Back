using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    [Table("training")]
    internal class Training
    {
        [Key]
        public int ID { get; set; }

        // Foreign Key voor Kalender
        public int KalenderID { get; set; }

        // Foreign Key voor Speler
        public int SpelerID { get; set; }

        //[ForeignKey("KalenderID")]
        //public Match Kalender { get; set; }

        //[ForeignKey("SpelerID")]
        //public Speler Speler { get; set; }

        public int? Uur { get; set; }
    }
}
