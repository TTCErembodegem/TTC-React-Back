using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class ClubLokaal
    {
        [Key]
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string Lokaal { get; set; }
        public string Adres { get; set; }
        public string Gemeente { get; set; }
        public int? Hoofd { get; set; }
        public int? Postcode { get; set; }
        public string Telefoon { get; set; }
        public ClubEntity Club { get; set; }

        public override string ToString()
        {
            return $"Id={Id}, ClubId={ClubId}, Lokaal={Lokaal}, Adres={Adres}, Gemeente={Gemeente}";
        }
    }
}
