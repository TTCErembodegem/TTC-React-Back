using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Ttc.DataAccess;
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
        [Category("Integration")]
        [Description("Find players in the database that would make AutoMapper fail silently")]
        public void FindInvalidPlayerRecords()
        {
            using (var dbContext = new TtcDbContext())
            {
                foreach (Speler speler in dbContext.Spelers)
                {
                    if (speler.ClubIdVttl.HasValue)
                    {
                        Assert.That(speler.ClubIdVttl, Is.Not.Null);
                        Assert.That(speler.IndexVttl, Is.Not.Null);
                        Assert.That(speler.LinkKaartVttl, Is.Not.Null);
                        Assert.That(speler.KlassementVttl, Is.Not.Null);
                        Assert.That(speler.ComputerNummerVttl, Is.Not.Null);
                        Assert.That(speler.VolgnummerVttl, Is.Not.Null);
                    }
                    if (speler.ClubIdSporta.HasValue)
                    {
                        Assert.That(speler.ClubIdSporta, Is.Not.Null);
                        Assert.That(speler.IndexSporta, Is.Not.Null);
                        Assert.That(speler.LinkKaartSporta, Is.Not.Null);
                        Assert.That(speler.KlassementSporta, Is.Not.Null);
                        Assert.That(speler.LidNummerSporta, Is.Not.Null);
                        Assert.That(speler.VolgnummerSporta, Is.Not.Null);
                    }
                }
            }
        }

        [Test]
        public void ReeksMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                GlobalBackendConfiguration.ConfigureAutoMapper(new KlassementValueConverter());

                var serv = new DivisionService();
                var club = serv.GetForCurrentYear();
                Assert.That(club.First().Year, Is.EqualTo(Constants.CurrentSeason));
            }
        }

        [Test]
        public void CalendarMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                GlobalBackendConfiguration.ConfigureAutoMapper(new KlassementValueConverter());

                var dateBegin = DateTime.Now.AddDays(-7);
                var dateEnd = DateTime.Now.AddDays(7);

                var calendar = dbContext.Kalender
                    .Where(x => x.Datum >= dateBegin)
                    .Where(x => x.Datum <= dateEnd)
                    .ToList();


                //var club = Mapper.Map<IList<ClubEntity>, IList<Club>>(clubs);
                //Assert.That(club.First().MainLocation, Is.Not.Null);
            }
        }

        [Test]
        public void ClubMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                GlobalBackendConfiguration.ConfigureAutoMapper(new KlassementValueConverter());

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
                GlobalBackendConfiguration.ConfigureAutoMapper(new KlassementValueConverter());
                Speler speler = dbContext.Spelers.First();
                Player player = Mapper.Map<Speler, Player>(speler);
                Assert.That(player.Vttl, Is.Not.Null);
            }
        }
    }
}