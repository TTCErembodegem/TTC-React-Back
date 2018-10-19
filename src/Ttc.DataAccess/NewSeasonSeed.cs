using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Frenoy.Api;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.DataAccess
{
    internal static class NewSeasonSeed
    {
        [Obsolete("Seed no longer works from Migrations. Add currentSeason to db and configure from frontend!")]
        public static void Seed(TtcDbContext context, bool clearMatches)
        {
            // TODO: Season 2019: Add GetOpponentMatches to initial seed (remove from MatchService)

            if (clearMatches)
            {
                context.Database.ExecuteSqlCommand("DELETE FROM matchplayer");
                context.Database.ExecuteSqlCommand("DELETE FROM matchgame");
                context.Database.ExecuteSqlCommand("DELETE FROM matchcomment");
                context.Database.ExecuteSqlCommand("DELETE FROM matches");
            }

            if (!context.Matches.Any(x => x.FrenoySeason == Constants.FrenoySeason))
            {
                var vttlPlayers = new FrenoyPlayersApi(context, Competition.Vttl);
                vttlPlayers.StopAllPlayers(true).RunSynchronously();
                vttlPlayers.SyncPlayers().RunSynchronously();
                var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
                sportaPlayers.SyncPlayers().RunSynchronously();

                var vttl = new FrenoyMatchesApi(context, Competition.Vttl);
                vttl.SyncTeamsAndMatches().RunSynchronously();
                var sporta = new FrenoyMatchesApi(context, Competition.Sporta);
                sporta.SyncTeamsAndMatches().RunSynchronously();
            }

            CreateSystemUser(context);
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