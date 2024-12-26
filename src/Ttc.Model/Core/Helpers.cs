namespace Ttc.Model.Core;

internal static class Helpers
{
    /// <summary>
    /// A match is considered started this time before the official start time
    /// </summary>
    private static readonly TimeSpan MatchStarts = new(0, 30, 0);
    public static bool HasMatchStarted(DateTime date) => date - DateTime.Now < MatchStarts;
}
