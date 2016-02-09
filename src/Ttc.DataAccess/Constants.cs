using System;

namespace Ttc.DataAccess
{
    internal class Constants
    {
        public const int OwnClubId = 1;
        /// <summary>
        /// Id van Dirk DS
        /// </summary>
        public const int SuperPlayerId = 4;
        public const string Sporta = "Sporta";
        public const string Vttl = "Vttl";
        /// <summary>
        /// Don't change this to 2016; Calculate it instead!
        /// </summary>
        public const int CurrentSeason = 2015;

        private static bool IsVttl(string value) => string.Equals(value.Trim(), Vttl, StringComparison.InvariantCultureIgnoreCase);

        public static string NormalizeCompetition(string value) => IsVttl(value) ? Vttl : Sporta;
    }
}