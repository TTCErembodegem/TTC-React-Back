using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class SpelerWeek
    {
        public int Id { get; set; }

        // Foreign Key voor Speler
        public int SpelerId { get; set; }

        [ForeignKey("SpelerId")]
        public Speler Speler { get; set; }

        public string Beschrijving { get; set; }

        public DateTime Datum { get; set; }

    }
}
