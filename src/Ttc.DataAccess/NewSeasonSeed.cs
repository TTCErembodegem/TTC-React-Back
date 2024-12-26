using Frenoy.Api;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Ttc.DataAccess
{
    internal static class NewSeasonSeed
    {
        /// <summary>
        /// Adds the matches and syncs the players for the new season
        /// </summary>
        /// <returns>The new season year</returns>
        public static async Task<int> Seed(ITtcDbContext context, bool clearMatches)
        {
            //if (clearMatches)
            //{
            //    context.Database.ExecuteSqlCommand("DELETE FROM matchplayer");
            //    context.Database.ExecuteSqlCommand("DELETE FROM matchgame");
            //    context.Database.ExecuteSqlCommand("DELETE FROM matchcomment");
            //    context.Database.ExecuteSqlCommand("DELETE FROM matches");
            //}

            int newYear = DateTime.Today.Year;
            int newFrenoyYear = newYear - 2000 + 1;
            if (DateTime.Today.Month < 7)
            {
                throw new Exception($"Starting new season {newYear}? That doesn't seem right?");
            }
            if (!context.Matches.Any(x => x.FrenoySeason == newFrenoyYear))
            {
                // VTTL
                //var vttlPlayers = new FrenoyPlayersApi(context, Competition.Vttl);
                //await vttlPlayers.StopAllPlayers(false);
                //await vttlPlayers.SyncPlayers();

                var vttl = new FrenoyMatchesApi(context, Competition.Vttl);
                await vttl.SyncTeamsAndMatches();


                // Sporta
                //var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
                //await sportaPlayers.StopAllPlayers(false);
                //await sportaPlayers.SyncPlayers();

                var sporta = new FrenoyMatchesApi(context, Competition.Sporta);
                await sporta.SyncTeamsAndMatches();
            }

            return newYear;
        }
    }
}