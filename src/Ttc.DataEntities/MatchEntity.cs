using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Ttc.Model.Players;

namespace Ttc.DataEntities;

[Table("matches")]
[Index(nameof(Date))]
public class MatchEntity
{
    [Key]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    /// Op vrije weken moet er niet gespeeld worden.
    /// Maar voor de weekopstelling moet er voor vrije ploegen ook een opstelling gebeuren.
    /// </summary>
    public bool ShouldBePlayed { get; set; }

    public int Week { get; set; }
    [MaxLength(20)]
    public string? FrenoyMatchId { get; set; }
    [MaxLength(200)]
    public string? Block { get; set; }

    //[Index]
    public int FrenoyDivisionId { get; set; }
    /// <summary>
    /// 2015-2016 = 16
    /// </summary>
    public int FrenoySeason { get; set; } // TODO: need extra filtering on season in frontend
    public Competition Competition { get; set; }

    public int? HomeTeamId { get; set; }
    [ForeignKey("HomeTeamId")]
    public TeamEntity HomeTeam { get; set; }
    //[Index]
    public int HomeClubId { get; set; }
    [MaxLength(2)]
    public string? HomeTeamCode { get; set; }

    public int? AwayTeamId { get; set; }
    [ForeignKey("AwayTeamId")]
    public TeamEntity AwayTeam { get; set; }
    //[Index]
    public int AwayClubId { get; set; }

    [MaxLength(2)]
    public string? AwayTeamCode { get; set; }

    public int ReportPlayerId { get; set; }
    public ICollection<MatchGameEntity> Games { get; set; }
    public ICollection<MatchPlayerEntity> Players { get; set; }
    public string? FormationComment { get; set; }
    public ICollection<MatchCommentEntity> Comments { get; set; }
    public string? Description { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public bool WalkOver { get; set; }
    public bool IsSyncedWithFrenoy { get; set; }

    /// <summary>
    /// Null when TTC Aalst did not play (~ ReadonlyMatch)
    /// True/False: Was TTC Aalst, True=Was in Aalst
    /// </summary>
    public bool? IsHomeMatch => !HomeTeamId.HasValue && !AwayTeamId.HasValue ? (bool?)null : HomeTeamId.HasValue;

    public MatchEntity()
    {
        Players = new List<MatchPlayerEntity>();
        Games = new List<MatchGameEntity>();
        Comments = new List<MatchCommentEntity>();
    }

    public override string ToString()
    {
        var str = $"Id={Id}, Date={Date:d} {Date:hh\\:mm}";
        return $"{str}, Club={AwayClubId}, Team={AwayTeamCode}";
    }
}
