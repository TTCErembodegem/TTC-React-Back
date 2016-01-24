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
using Ttc.Model.Players;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class PlaygroundTests
    {
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
                AutoMapperConfig.Configure(new KlassementValueConverter());

                var serv = new CalendarService();
                var result = serv.GetRelevantCalendarItems();
                Assert.That(result.First().FrenoyMatchId, Is.Not.Null);
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