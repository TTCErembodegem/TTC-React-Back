using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Entities
{
    internal class ClubPloegSpeler
    {
        [Key]
        public int Id { get; set; }
        public int? ClubPloegId { get; set; }
        public int? SpelerId { get; set; }
        public TeamPlayerType Kapitein { get; set; }
        public ClubPloeg Ploeg { get; set; }

        public override string ToString()
        {
            return $"Id={Id}, ClubPloegId={ClubPloegId}, SpelerId={SpelerId}, Kapitein={Kapitein}";
        }
    }
}
