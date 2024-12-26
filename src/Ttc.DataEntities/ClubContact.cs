using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[PrimaryKey(nameof(ClubId), nameof(SpelerId))]
[Table("clubcontact")]
public class ClubContact
{
    public int ClubId { get; set; }
    public int SpelerId { get; set; }

    public ClubEntity Club { get; set; }
    public string Omschrijving { get; set; }
    public int Sortering { get; set; }

    public override string ToString() => $"Club={ClubId}, Desc={Omschrijving}, Sort={Sortering}, Player={SpelerId}";
}
