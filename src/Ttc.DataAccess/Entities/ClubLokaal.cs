using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class ClubLokaal
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key voor Club
        public int ClubId { get; set; }

        //[ForeignKey("ClubId")]
        //public Club Club { get; set; }

        public string Lokaal { get; set; }

        public string Adres { get; set; }

        public string Gemeente { get; set; }

        public int? Hoofd { get; set; }

        public int? Postcode { get; set; }

        public string Telefoon { get; set; }
    }
}
