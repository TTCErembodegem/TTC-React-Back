using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities
{
    public class Reeks
    {
        [Key]
        public int Id { get; set; }
        public string Competitie { get; set; }
        [Column("Reeks")]
        public string ReeksNummer { get; set; }
        public string ReeksType { get; set; }
        public string ReeksCode { get; set; }
        public int? Jaar { get; set; }
        public string LinkId { get; set; }
        public string FrenoyTeamId { get; set; }
        public int FrenoyDivisionId { get; set; }

        public ICollection<MatchEntity> Matchen { get; set; }
        public ICollection<ClubPloegSpeler> Spelers { get; set; }
        public ICollection<ClubPloeg> Opponents { get; set; }

        /// <summary>
        /// TTC Erembodegem TeamCode (A, B, C, ...)
        /// </summary>
        public string TeamCode { get; set; }

        public override string ToString() => $"Id={Id}, Competitie={Competitie} {Jaar}, Reeks={ReeksNummer}{ReeksCode}, FrenoyLink={LinkId}";
    }
}
