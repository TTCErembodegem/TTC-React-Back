using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class ClubContact
    {
        [Key, Column(Order = 0)]
        public int ClubId { get; set; }
        [Key, Column(Order = 1)]
        public int SpelerId { get; set; }

        public ClubEntity Club { get; set; }
        public string Omschrijving { get; set; }
        public int Sortering { get; set; }

        public override string ToString() => $"Club={ClubId}, Desc={Omschrijving}, Sort={Sortering}, Player={SpelerId}";
    }
}
