namespace Ttc.Model.Clubs;

public class Club
{
    #region Properties
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? CodeVttl { get; set; }
    public string? CodeSporta { get; set; }
    public bool Active { get; set; }
    public bool Shower { get; set; }
    public string Website { get; set; } = "";

    public ClubLocation? MainLocation { get; set; }
    public ICollection<ClubLocation> AlternativeLocations { get; set; } = [];

    /// <summary>
    /// Voorzitter, secretaris, ...
    /// </summary>
    public ICollection<ClubManager>? Managers { get; set; }
    #endregion

    public override string ToString() => $"Id={Id}, Name={Name}, Vttl={CodeVttl}, Sporta={CodeSporta}, Active={Active}";
}
