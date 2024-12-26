using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Table("matchcomment")]
public class MatchCommentEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime PostedOn { get; set; }
    public string? Text { get; set; }
    public bool Hidden { get; set; }
    [MaxLength(100)]
    public string? ImageUrl { get; set; }

    [ForeignKey("MatchId")]
    public MatchEntity Match { get; set; }
    public int MatchId { get; set; }
    public int PlayerId { get; set; }

    public override string ToString() => $"Id: {Id}, Text: {Text}, MatchId: {MatchId}, PlayerId: {PlayerId}";
}
