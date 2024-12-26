namespace Ttc.Model.Matches;

public static class PlayerMatchStatus
{
    /// <summary>
    /// Opstelling door ploeg kapitein
    /// </summary>
    public const string Captain = "Captain";

    /// <summary>
    /// Opstelling door de kapiteins der kapiteins:)
    /// Frenoy sync zet ook deze status
    /// </summary>
    public const string Major = "Major";

    // Opstelling door de spelers zelf:
    public const string Play = "Play";
    public const string NotPlay = "NotPlay";
    public const string Maybe = "Maybe";
    public const string DontKnow = "DontKnow";
}
