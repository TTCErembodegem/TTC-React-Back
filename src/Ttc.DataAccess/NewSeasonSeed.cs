using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Frenoy.Api;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.DataAccess
{
    internal static class NewSeasonSeed
    {
        /// <summary>
        /// Adds the matches and syncs the players for the new season
        /// </summary>
        /// <returns>The new season year</returns>
        public static async Task<int> Seed(TtcDbContext context, bool clearMatches)
        {
            // TODO: Season 2020: Add GetOpponentMatches to initial seed (remove from MatchService)

            //if (clearMatches)
            //{
            //    context.Database.ExecuteSqlCommand("DELETE FROM matchplayer");
            //    context.Database.ExecuteSqlCommand("DELETE FROM matchgame");
            //    context.Database.ExecuteSqlCommand("DELETE FROM matchcomment");
            //    context.Database.ExecuteSqlCommand("DELETE FROM matches");
            //}

            //int newYear = context.CurrentFrenoySeason + 1;
            int newYear = 2019;
            if (!context.Matches.Any(x => x.FrenoySeason == newYear))
            {
                var vttlPlayers = new FrenoyPlayersApi(context, Competition.Vttl);
                await vttlPlayers.StopAllPlayers(true);
                await vttlPlayers.SyncPlayers();

                var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
                await sportaPlayers.StopAllPlayers(true);
                await sportaPlayers.SyncPlayers();

                //var vttl = new FrenoyMatchesApi(context, Competition.Vttl);
                //await vttl.SyncTeamsAndMatches();
                //var sporta = new FrenoyMatchesApi(context, Competition.Sporta);
                //await sporta.SyncTeamsAndMatches();
            }

            //CreateSystemUser(context);

            return newYear;
        }

        private static void CreateSystemUser(TtcDbContext context)
        {
            // Clublokaal user account
            context.Players.AddOrUpdate(p => p.NaamKort, new PlayerEntity
            {
                Gestopt = 1,
                FirstName = "SYSTEM",
                NaamKort = "SYSTEM",
                Toegang = PlayerToegang.System
            });
            //context.Database.ExecuteSqlCommand("UPDATE speler SET paswoord=MD5('system') WHERE FirstName='SYSTEM' AND paswoord IS NULL");
        }
    }
}