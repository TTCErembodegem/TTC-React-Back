using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Players;

namespace Ttc.DataEntities;
[Table("team")]
public class TeamEntity
{
    [Key]
    public int Id { get; set; }
    [MaxLength(10)]
    public string Competition { get; set; }
    /// <summary>
    /// ColumnName "Reeks" in the db
    /// </summary>
    [Column("Reeks")]
    [MaxLength(2)]
    public string ReeksNummer { get; set; }
    [MaxLength(10)]
    public string ReeksType { get; set; }
    [MaxLength(2)]
    public string ReeksCode { get; set; }
    public int Year { get; set; }
    [MaxLength(10)]
    public string LinkId { get; set; }
    [MaxLength(10)]
    public string FrenoyTeamId { get; set; }
    public int FrenoyDivisionId { get; set; }

    public Competition GetCompetition()
    {
        return Constants.NormalizeCompetition(Competition);
    }

    public ICollection<TeamPlayerEntity> Players { get; set; }
    public ICollection<TeamOpponentEntity> Opponents { get; set; }

    /// <summary>
    /// TTC Aalst TeamCode (A, B, C, ...)
    /// </summary>
    [MaxLength(2)]
    public string TeamCode { get; set; }

    public override string ToString() => $"Id={Id}, Competitie={Competition} {Year}, Reeks={ReeksNummer}{ReeksCode}, FrenoyLink={LinkId}";
}
