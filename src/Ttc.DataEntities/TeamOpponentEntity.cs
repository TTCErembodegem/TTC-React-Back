using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Table("teamopponent")]
public class TeamOpponentEntity
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("TeamId")]
    public TeamEntity Team { get; set; }
    public int TeamId { get; set; }

    [ForeignKey("ClubId")]
    public ClubEntity Club { get; set; }
    public int ClubId { get; set; }

    /// <summary>
    /// Team A, B, C, ...
    /// </summary>
    [MaxLength(2)]
    public string TeamCode { get; set; }

    public override string ToString() => $"Id={Id}, Reeks=_{Team}_, ClubId={ClubId}, TeamCode={TeamCode}";
}
