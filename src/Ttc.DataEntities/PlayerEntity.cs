using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Flags]
public enum PlayerToegang
{
    Player = 1,
    /// <summary>
    /// Dirk DS, ...
    /// </summary>
    Board = 7,
    /// <summary>
    /// Jorn en ik
    /// </summary>
    Dev = 8,
    /// <summary>
    /// Algemeen toegankelijke computer in het clublokaal
    /// </summary>
    System = 9,
}

[Table(TableName)]
public class PlayerEntity
{
    public const string TableName = "speler";

    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }
    [StringLength(100)]
    public string? LastName { get; set; }
    /// <summary>
    /// FirstName + LastName
    /// </summary>
    [NotMapped]
    public string Name => $"{FirstName} {LastName}";

    public string? NaamKort { get; set; }
    public PlayerToegang Toegang { get; set; }
    public string? Stijl { get; set; }
    public string? BesteSlag { get; set; }

    /// <summary>
    /// Has a key to enter the physical club
    /// </summary>
    public bool? HasKey { get; set; }

    #region Vttl
    public int? ClubIdVttl { get; set; }
    public int? IndexVttl { get; set; }
    public string? LinkKaartVttl { get; set; }
    public string? KlassementVttl { get; set; }
    public string? NextKlassementVttl { get; set; }
    public int? ComputerNummerVttl { get; set; }
    public int? VolgnummerVttl { get; set; }
    #endregion

    #region Sporta
    public int? ClubIdSporta { get; set; }
    public int? IndexSporta { get; set; }
    public string? LinkKaartSporta { get; set; }
    public string? KlassementSporta { get; set; }
    public string? NextKlassementSporta { get; set; }
    public int? LidNummerSporta { get; set; }
    public int? VolgnummerSporta { get; set; }
    #endregion

    #region Address
    public string? Adres { get; set; }
    public string? Gemeente { get; set; }
    public string? Gsm { get; set; }
    public string? Email { get; set; }
    #endregion

    /// <summary>
    /// Jaar gestopt. <see cref="IsGestopt"/>?
    /// </summary>
    public int? Gestopt { get; set; }
    public bool IsGestopt => Gestopt != null;

    public bool IsFromOwnClub() => ClubIdSporta == Constants.OwnClubId || ClubIdVttl == Constants.OwnClubId;

    public override string ToString() => $"Id={Id}, Alias={NaamKort} ({ClubIdVttl}, {ClubIdSporta}), Vttl={KlassementVttl}, Sporta={KlassementSporta}, IsGestopt={IsGestopt}";
}
