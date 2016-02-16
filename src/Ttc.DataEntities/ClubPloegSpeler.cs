using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Teams;

namespace Ttc.DataEntities
{
    public class ClubPloegSpeler
    {
        [Key]
        public int Id { get; set; }
        public TeamPlayerType Kapitein { get; set; }

        [ForeignKey("SpelerId")]
        public Speler Speler { get; set; }
        public int? SpelerId { get; set; }

        [ForeignKey("ClubPloegId")]
        public ClubPloeg Ploeg { get; set; }
        public int? ClubPloegId { get; set; }

        public override string ToString() => $"Id={Id}, ClubPloegId={ClubPloegId}, SpelerId={SpelerId}, Kapitein={Kapitein}";
    }
}
