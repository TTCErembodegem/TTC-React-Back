using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    [Table("verslagspeler")]
    internal class VerslagSpeler
    {
        [Key]
        public int ID { get; set; }

        // Foreign Key voor Verslag
        public int VerslagID { get; set; }
        
        // Foreign Key voor Speler
        public int SpelerID { get; set; }

        [ForeignKey("VerslagID")]
        public Verslag Verslag { get; set; }

        [ForeignKey("SpelerID")]
        public Speler Speler { get; set; }

        public int? Winst { get; set; }

        public string SpelerNaam { get; set; }

        public int? Thuis { get; set; }

        public string Klassement { get; set; }
    }
}
