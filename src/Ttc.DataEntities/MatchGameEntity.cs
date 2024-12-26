using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Matches;

namespace Ttc.DataEntities;

[Table("matchgame")]
public class MatchGameEntity
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("MatchId")]
    public MatchEntity Match { get; set; }
    public int MatchId { get; set; }

    public int MatchNumber { get; set; }
    public int HomePlayerUniqueIndex { get; set; }
    public int HomePlayerSets { get; set; }
    public int AwayPlayerUniqueIndex { get; set; }
    public int AwayPlayerSets { get; set; }
    public WalkOver WalkOver { get; set; }

    public override string ToString() => $"Id={MatchId}, Match#={MatchNumber}, ThuisSpeler={HomePlayerUniqueIndex}:{AwayPlayerUniqueIndex}, UitSpeler={HomePlayerSets}:{AwayPlayerSets}";
}
