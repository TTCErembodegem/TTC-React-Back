using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Clubs;

namespace Ttc.DataEntities;

/// <summary>
/// Entity suffix: Otherwise conflict with <see cref="Club"/>
/// </summary>
[Table("club")]
public class ClubEntity
{
    [Key]
    public int Id { get; set; }
    public string Naam { get; set; } = "";
    public string? CodeVttl { get; set; }
    public string? CodeSporta { get; set; }
    public int Actief { get; set; }
    public int Douche { get; set; }
    public string? Website { get; set; }

    public ICollection<ClubLokaal> Lokalen { get; set; }

    public ICollection<ClubContact> Contacten { get; set; }

    public override string ToString() => $"Id={Id}, Name={Naam}, Vttl={CodeVttl}, Sporta={CodeSporta}, Active={Actief}";
}
