using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataAccess.Entities
{
    internal class Club
    {
        [Key]
        public int Id { get; set; }

        public string Naam { get; set; }

        public string CodeVttl { get; set; }

        public int? Actief { get; set; }

        public int? Douche { get; set; }

        public string Website { get; set; }

        public string CodeSporta { get; set; }
    }
}
