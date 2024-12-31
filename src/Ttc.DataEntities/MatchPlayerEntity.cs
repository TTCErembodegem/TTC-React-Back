using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Table("matchplayer")]
public class MatchPlayerEntity
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("MatchId")]
    public MatchEntity Match { get; set; }
    public int MatchId { get; set; }

    [ForeignKey("PlayerId")]
    public PlayerEntity Player { get; set; }
    public int PlayerId { get; set; }

    [MaxLength(10)]
    public string Status { get; set; }
    [MaxLength(300)]
    public string? StatusNote { get; set; }

    /// <summary>
    /// Aantal gewonnen matchen.
    /// Null => Forfeit
    /// </summary>
    public int? Won { get; set; }
    public bool Home { get; set; }

    public int Position { get; set; }

    // TODO: Name & UniqueIndex are required (and non 0) or the automapper crashes (some attribute?)
    [MaxLength(50)]
    public string Name { get; set; }
    [MaxLength(5)]
    public string Ranking { get; set; }
    public int UniqueIndex { get; set; }

    public override string ToString() => $"MatchId={MatchId}, Name={Name}, Won={Won}, Home={Home}, Status={Status}";
}
