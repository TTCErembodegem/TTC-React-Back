using System.ComponentModel.DataAnnotations;
using Ttc.Model.Core;

namespace Ttc.Model.Players;

public class Player : ITtcConfidential
{
    #region Properties
    public int Id { get; set; }
    [StringLength(100)]
    public string FirstName { get; set; } = "";
    [StringLength(100)]
    public string LastName { get; set; } = "";

    public string Alias { get; set; } = "";
    public bool Active { get; set; }
    public int? QuitYear { get; set; }

    public string Security { get; set; } = "";
    public bool? HasKey { get; set; }

    public PlayerStyle? Style { get; set; }

    public PlayerContact? Contact { get; set; }

    public PlayerCompetition? Vttl { get; set; }
    public PlayerCompetition? Sporta { get; set; }
    #endregion

    public void Hide()
    {
        Security = "";
        HasKey = null;
        Contact = null;
    }

    public PlayerCompetition? GetCompetition(Competition competition) => competition == Competition.Vttl ? Vttl : Sporta;

    public override string ToString() => $"Id={Id}, Alias={Alias}, Active={Active}";
}
