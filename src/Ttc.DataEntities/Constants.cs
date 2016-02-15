using System;
using Ttc.Model.Players;

namespace Ttc.DataEntities
{
    public static class Constants
    {
        public const int OwnClubId = 1;

        /// <summary>
        /// Id van Dirk DS
        /// </summary>
        public const int SuperPlayerId = 4;

        /// <summary>
        /// A match is considered started this time before the official start time
        /// </summary>
        private static readonly TimeSpan MatchStarts = new TimeSpan(0, 30, 0);
        public static bool HasMatchStarted(DateTime date)
        {
            return date - DateTime.Now < MatchStarts;
        }

        #region Competition
        public const string Sporta = "Sporta";
        public const string Vttl = "Vttl";
        /// <summary>
        /// Don't change this to 2016; Calculate it instead!
        /// </summary>
        public const int CurrentSeason = 2015;

        private static bool IsVttl(string value) => string.Equals(value.Trim(), Vttl, StringComparison.InvariantCultureIgnoreCase);

        public static Competition NormalizeCompetition(string value) => IsVttl(value) ? Competition.Vttl : Competition.Sporta;
        #endregion
    }
}