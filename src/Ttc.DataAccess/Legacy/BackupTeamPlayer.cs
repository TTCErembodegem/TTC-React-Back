using System.ComponentModel.DataAnnotations;

namespace Ttc.DataAccess.Legacy;

internal class BackupTeamPlayer
{
    [Key]
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string DivisionLinkId { get; set; }
    public string TeamCode { get; set; }

    public override string ToString() => $"PlayerId: {PlayerId}, DivisionLinkId: {DivisionLinkId}, TeamCode: {TeamCode}";
}
