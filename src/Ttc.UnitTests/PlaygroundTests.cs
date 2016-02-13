using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataAccess.App_Start;
using Ttc.DataAccess.Entities;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Matches;
using Ttc.Model.Players;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class PlaygroundTests
    {
        //[Test]
        //public void KalenderToVerslag()
        //{
        //    using (var dbContext = new TtcDbContext())
        //    {
        //        var calendar = dbContext.Kalender
        //            .Include(x => x.ThuisClubPloeg)
        //            .Include(x => x.Verslag)
        //            .Where(x => x.Verslag != null)
        //            .ToList();

        //        //var result = Mapper.Map<IList<Kalender>, IList<Match>>(calendar);
        //        Assert.That(calendar.Any());
        //    }
        //}

        [Test]
        public void ReeksMapping()
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
                //var pastMatchEntity = dbContext.Kalender
                //    .Include(x => x.Verslag)
                //    .Include("Verslag.Spelers")
                //    .Single(x => x.Id == 1597);
                //Assert.That(pastMatchEntity.Verslag, Is.Not.Null);

                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new CalendarService();
                var result = serv.GetRelevantMatches();
                var pastMatch = result.First();
                //var pastMatch = serv.GetMatch(1597);
                Assert.That(pastMatch.Players.Count, Is.Not.EqualTo(0), "Players not loaded");
                Assert.That(pastMatch.Games.Count, Is.Not.EqualTo(0), "Games not loaded");
                Assert.That(pastMatch.Score, Is.Not.Null, "Score not set");
            }
        }

        [Test]
        public void TeamMapping()
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
                Speler speler = dbContext.Spelers.First();
                Player player = Mapper.Map<Speler, Player>(speler);
                Assert.That(player.Vttl, Is.Not.Null);
            }
        }
    }
}