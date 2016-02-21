using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities
{
    [Table("match")]
    public class MatchEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public int Week { get; set; }
        public string FrenoyMatchId { get; set; }

        public int? HomeTeamId { get; set; }
        [ForeignKey("HomeTeamId")]
        public TeamEntity HomeTeam { get; set; }
        public int HomeClubId { get; set; }
        public string HomeTeamCode { get; set; }

        public int? AwayTeamId { get; set; }
        [ForeignKey("AwayTeamId")]
        public TeamEntity AwayTeam { get; set; }
        public int AwayClubId { get; set; }
        public string AwayPloegCode { get; set; } // TODO: rename to AwayTeamCode

        public int ReportPlayerId { get; set; }
        public ICollection<MatchGameEntity> Games { get; set; }
        public ICollection<MatchPlayerEntity> Players { get; set; }
        public string Description { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public bool WalkOver { get; set; }
        public bool IsSyncedWithFrenoy { get; set; }
        public bool? IsHomeMatch => !HomeTeamId.HasValue && !AwayTeamId.HasValue ? (bool?)null : HomeTeamId.HasValue;

        public override string ToString()
        {
            var str = $"Id={Id}, Date={Date.ToString("d")} {Date.ToString(@"hh\:mm")}";
            return $"{str}, UitClubId={AwayClubId}, UitPloeg={AwayPloegCode}";
        }
    }
}
