using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Frenoy.Api;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataAccess.App_Start;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class PlaygroundTests
    {
        [Test]
        public void FillTeamPlayers()
        {
            var settings = FrenoySettings.VttlSettings;
            var comp = Competition.Vttl;

            using (var dbContext = new TtcDbContext())
            {
                foreach (KeyValuePair<string, string[]> dict in settings.Players)
                {
                    TeamEntity team = dbContext.Teams
                        .Where(x => x.Year == settings.Year)
                        .Single(x => x.Competition == comp.ToString() && x.TeamCode == dict.Key);

                    foreach (string player in dict.Value)
                    {
                        var newPlayer = new TeamPlayerEntity
                        {
                            TeamId = team.Id,
                            PlayerType = TeamPlayerType.Standard,
                            PlayerId = dbContext.Players.Single(x => x.NaamKort == player).Id
                        };
                        dbContext.TeamPlayers.Add(newPlayer);
                    }
                }
                dbContext.SaveChanges();
            }
        }

        [Test]
        public void TeamMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new TeamService();
                var club = serv.GetForCurrentYear();
                Assert.That(club.First().Year, Is.EqualTo(Constants.CurrentSeason));
            }
        }

        [Test]
        public void CalendarMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new MatchService();
                var result = serv.GetRelevantMatches();
                var pastMatch = result.First();
                //var pastMatch = serv.GetMatch(1597);
                Assert.That(pastMatch.Players.Count, Is.Not.EqualTo(0), "Players not loaded");
                Assert.That(pastMatch.Games.Count, Is.Not.EqualTo(0), "Games not loaded");
                Assert.That(pastMatch.Score, Is.Not.Null, "Score not set");
            }
        }

        [Test]
        public void TeamMapping2()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new TeamService();
                var result = serv.GetForCurrentYear();
                var pastMatch = result.First();
                Assert.That(pastMatch.Players.Count, Is.Not.EqualTo(0));
                Assert.That(pastMatch.Opponents.Count, Is.Not.EqualTo(0));
            }
        }

        [Test]
        public void ClubMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var clubs = dbContext.Clubs.Include(x => x.Contacten).Where(x => x.Id == 1 || x.Id == 28).ToList();
                var club = Mapper.Map<IList<ClubEntity>, IList<Club>>(clubs);
                Assert.That(club.First().MainLocation, Is.Not.Null);
                Assert.That(club.First().Managers.Count, Is.Not.EqualTo(0));
            }
        }

        [Test]
        public void SpelerToPlayerMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                AutoMapperConfig.Configure(new KlassementValueConverter());
                PlayerEntity speler = dbContext.Players.First();
                Player player = Mapper.Map<PlayerEntity, Player>(speler);
                Assert.That(player.Vttl, Is.Not.Null);
            }
        }
    }
}