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

        public static void Seed(TtcDbContext context, bool clearMatches, bool syncTeamPlayers)
        {
            //UPDATE speler SET email="guyjacobs7@gmail.com" where id=28;
            //UPDATE speler set email="etienne.cornu@telenet.be" where id=30;
            //UPDATE speler set email="hugo.redant@telenet.be" where id=79;


            if (clearMatches)
            {
                context.Database.ExecuteSqlCommand("DELETE FROM matchplayer");
                context.Database.ExecuteSqlCommand("DELETE FROM matchgame");
                context.Database.ExecuteSqlCommand("DELETE FROM matchcomment");
                context.Database.ExecuteSqlCommand("DELETE FROM matches");
            }
            if (syncTeamPlayers)
            {
                context.Database.ExecuteSqlCommand("DELETE FROM teamplayer");
            }

            if (!context.Matches.Any())
            {
                var vttl = new FrenoyMatchesApi(context, Competition.Vttl);
                vttl.SyncTeamsAndMatches();
                if (syncTeamPlayers)
                {
                    AddTeamPlayers(context, FrenoySettings.VttlSettings);
                }

                var sporta = new FrenoyMatchesApi(context, Competition.Sporta);
                sporta.SyncTeamsAndMatches();
                if (syncTeamPlayers)
                {
                    AddTeamPlayers(context, FrenoySettings.SportaSettings);
                }
            }
        }

        protected override void Seed(TtcDbContext context)
        {
#if DEBUG
            Seed(context, true, true);
#else
            Seed(context, false, false);
#endif

            // Clublokaal user account
            context.Players.AddOrUpdate(p => p.NaamKort, new PlayerEntity
            {
                Gestopt = 1,
                Naam = "SYSTEM",
                NaamKort = "SYSTEM",
                Toegang = PlayerToegang.System
            });
            context.Database.ExecuteSqlCommand("UPDATE speler SET paswoord=MD5('system') WHERE Naam='SYSTEM' AND paswoord IS NULL");
        }

        private static void AddTeamPlayers(TtcDbContext context, FrenoySettings settings)
        {
            foreach (KeyValuePair<string, string[]> dict in settings.Players)
            {
                TeamEntity team = context.Teams
                    .Where(x => x.Year == settings.Year)
                    .Single(x => x.Competition.ToUpper() == settings.Competition.ToString().ToUpper() && x.TeamCode == dict.Key);

                foreach (string player in dict.Value)
                {
                    var newPlayer = new TeamPlayerEntity
                    {
                        TeamId = team.Id,
                        PlayerType = TeamPlayerType.Standard,
                        PlayerId = context.Players.Single(x => x.NaamKort == player).Id
                    };
                    context.TeamPlayers.Add(newPlayer);
                }
            }
        }
    }
}
