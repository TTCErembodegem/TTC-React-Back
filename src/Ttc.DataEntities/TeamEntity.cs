using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities
{
    [Table("team")]
    public class TeamEntity
    {
        [Key]
        public int Id { get; set; }
        public string Competition { get; set; }
        [Column("Reeks")]
        public string ReeksNummer { get; set; }
        public string ReeksType { get; set; }
        public string ReeksCode { get; set; }
        public int Year { get; set; }
        public string LinkId { get; set; }
        public string FrenoyTeamId { get; set; }
        public int FrenoyDivisionId { get; set; }

        //public ICollection<MatchEntity> Matches { get; set; }
        public ICollection<TeamPlayerEntity> Players { get; set; }
        public ICollection<TeamOpponentEntity> Opponents { get; set; }

        /// <summary>
        /// TTC Erembodegem TeamCode (A, B, C, ...)
        /// </summary>
        public string TeamCode { get; set; }

        public override string ToString() => $"Id={Id}, Competitie={Competition} {Year}, Reeks={ReeksNummer}{ReeksCode}, FrenoyLink={LinkId}";
    }
}
