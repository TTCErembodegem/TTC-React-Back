using System.Data.Entity.Migrations;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataEntities;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        [Category("Integration")]
        public void MigrateToLatestVersion()
        {
            using (var dbContext = new TtcDbContext())
            {
                var config = new DbMigrationsConfiguration()
                {
                    MigrationsAssembly = typeof (TtcDbContext).Assembly,
                    ContextType = typeof (TtcDbContext),
                    MigrationsNamespace = "Ttc.DataAccess.Migrations"
                };

                var migrator = new DbMigrator(config);
                migrator.Update();
                //migrator.Update("0");
            }
        }

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
        [Category("Integration")]
        [Description("Remove trailing excess spaces in db columns")]
        public void FixExcessWhitespaceAndTelephoneNumbers()
        {
            using (var dbContext = new TtcDbContext())
            {
                foreach (var player in dbContext.Spelers)
                {
                    if (player.Email != null)
                        player.Email = player.Email.Trim();

                    if (player.Adres != null)
                        player.Adres = player.Adres.Trim();

                    if (player.Gemeente != null)
                        player.Gemeente = player.Gemeente.Trim();

                    //player.BesteSlag = player.BesteSlag.Trim();
                    //player.Stijl = player.Stijl.Trim();

                    player.Gsm = FixGsm(player.Gsm);
                }

                foreach (var player in dbContext.ClubLokalen)
                {
                    //player.Adres = player.Adres.Trim();
                    //player.Gemeente = player.Gemeente.Trim();
                    player.Telefoon = FixGsm(player.Telefoon);
                }
                dbContext.SaveChanges();
            }
        }

        private static string FixGsm(string gsm) => gsm?.Trim().Replace(" ", "").Replace("/", "").Replace(".", "");
    }
}