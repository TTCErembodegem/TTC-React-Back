using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities
{
    public class MatchPlayerEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("MatchId")]
        public MatchEntity Match { get; set; }
        public int MatchId { get; set; }

        [ForeignKey("PlayerId")]
        public Speler Speler { get; set; }
        public int PlayerId { get; set; }

        /// <summary>
        /// Aantal gewonnen matchen.
        /// Null => Forfeit
        /// </summary>
        public int? Won { get; set; }
        public bool Home { get; set; }

        public int Position { get; set; }
        public string Name { get; set; }
        public string Ranking { get; set; }
        public int UniqueIndex { get; set; }

        public override string ToString() => $"MatchId={MatchId}, NAme={Name}, Won={Won}, Home={Home}, Position={Position}";
    }
}
