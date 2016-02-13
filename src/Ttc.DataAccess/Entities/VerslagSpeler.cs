using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class VerslagSpeler
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("MatchId")]
        public Verslag Verslag { get; set; }
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

        public override string ToString()
        {
            return $"MatchId={MatchId}, NAme={Name}, Won={Won}, Home={Home}, Position={Position}";
        }
    }
}
