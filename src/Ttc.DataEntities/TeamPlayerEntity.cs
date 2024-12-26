using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Teams;

namespace Ttc.DataEntities;

[Table("teamplayer")]
public class TeamPlayerEntity
{
    [Key]
    public int Id { get; set; }
    public TeamPlayerType PlayerType { get; set; }

    [ForeignKey("PlayerId")]
    public PlayerEntity Player { get; set; }
    public int PlayerId { get; set; }

    [ForeignKey("TeamId")]
    public TeamEntity Team { get; set; }
    public int TeamId { get; set; }

    public override string ToString() => $"Id={Id}, Team={TeamId}, SpelerId={PlayerId}, Type={PlayerType}";
}
