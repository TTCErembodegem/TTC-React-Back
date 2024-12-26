using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Table("parameter")]
public class ParameterEntity
{
    [Key]
    public string Sleutel { get; set; }
    public string Value { get; set; }
    public string? Omschrijving { get; set; }

    public override string ToString() => $"{Sleutel}={Value}, Desc={Omschrijving}";
}
