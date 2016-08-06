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
        public const int JornPlayerId = 36;
        public const int WouterPlayerId = 20;

        #region Competition
        public const string Sporta = "Sporta";
        public const string Vttl = "Vttl";
        public const int CurrentSeason = 2016;
        public const int FrenoySeason = CurrentSeason - 2000 + 1;

        private static bool IsVttl(string value) => string.Equals(value.Trim(), Vttl, StringComparison.InvariantCultureIgnoreCase);

        public static Competition NormalizeCompetition(string value) => IsVttl(value) ? Competition.Vttl : Competition.Sporta;
        #endregion
    }
}