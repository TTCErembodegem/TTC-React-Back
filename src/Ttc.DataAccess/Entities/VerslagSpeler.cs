using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class VerslagSpeler
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("KalenderId")]
        public Verslag Verslag { get; set; }
        public int KalenderId { get; set; }

        [ForeignKey("SpelerId")]
        public Speler Speler { get; set; }
        public int SpelerId { get; set; }

        /// <summary>
        /// Aantal gewonnen matchen.
        /// Null => Forfeit
        /// </summary>
        public int? Winst { get; set; }
        public int Thuis { get; set; }

        public int Positie { get; set; }
        public string SpelerNaam { get; set; }
        public string Klassement { get; set; }
        public int UniqueIndex { get; set; }

        public override string ToString()
        {
            return $"KalenderId={KalenderId}, SpelerNaam={SpelerNaam}, Winst={Winst}, Thuis={Thuis}, Positie={Positie}";
        }
    }
}
