using System.Collections.Generic;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Utilities;
using Frenoy.Api;
using Ttc.DataEntities;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Ttc.DataAccess.TtcDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        public static void Seed(TtcDbContext context, bool clearMatches)
        {
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
                vttlPlayers.StopAllPlayers(true);
                vttlPlayers.SyncPlayers();
                var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
                sportaPlayers.SyncPlayers();

                var vttl = new FrenoyMatchesApi(context, Competition.Vttl);
                vttl.SyncTeamsAndMatches();
                var sporta = new FrenoyMatchesApi(context, Competition.Sporta);
                sporta.SyncTeamsAndMatches();
            }
        }

        protected override void Seed(TtcDbContext context)
        {
            Seed(context, false);

            // Clublokaal user account
            context.Players.AddOrUpdate(p => p.NaamKort, new PlayerEntity
            {
                Gestopt = 1,
                FirstName = "SYSTEM",
                NaamKort = "SYSTEM",
                Toegang = PlayerToegang.System
            });
            context.Database.ExecuteSqlCommand("UPDATE speler SET paswoord=MD5('system') WHERE Naam='SYSTEM' AND paswoord IS NULL");
        }
    }
}
