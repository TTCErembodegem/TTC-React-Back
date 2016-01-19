using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataAccess.Entities;
using Ttc.Model;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class PlaygroundTests
    {
        [Test]
        public void SpelerToPlayerMapping()
        {
            using (var dbContext = new TtcDbContext())
            {
                Speler speler = dbContext.Spelers.First();
                Player player = Mapper.Map<Speler, Player>(speler);
                Assert.That(player.Vttl, Is.Not.Null);
            }
        }
    }
}