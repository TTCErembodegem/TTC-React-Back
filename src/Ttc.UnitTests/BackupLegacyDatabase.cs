using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Frenoy.Api;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataAccess.Backup;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.UnitTests
{
    /// <summary>
    /// Git Tag:
    /// git co PRD-Backup-Script
    /// 
    /// Update-Database -TargetMigration:xxx
    /// 
    /// So that a backup can be made before starting with the new db...
    /// </summary>
    [TestFixture]
    public class BackupLegacyDatabase
    { 
        //[Test]
        //public void BackupMatchReports()
        //{
        //    int reportLostCount = 0;
        //    using (var dbContent = new TtcDbContext())
        //    {
        //        var matchesWithReport = dbContent.Kalender
        //            .Include(x => x.Verslag)
        //            .Where(x => x.Verslag != null && x.Verslag.Beschrijving != null && x.Verslag.Beschrijving != "")
        //            .Where(x => x.ThuisClubId.HasValue)
        //            .ToArray();

        //        foreach (var verslag in matchesWithReport)
        //        {
        //            string frenoyMatchId = verslag.FrenoyMatchId;
        //            if (string.IsNullOrWhiteSpace(frenoyMatchId))
        //            {
        //                var ploeg = dbContent.ClubPloegen
        //                    .Include(x => x.Reeks)
        //                    .FirstOrDefault(x => x.Id == verslag.ThuisClubPloegId.Value);

        //                if (ploeg == null)
        //                {
        //                    // Derbys that are not correctly in the db anymore...
        //                    reportLostCount++;
        //                    continue;
        //                }

        //                var comp = Constants.NormalizeCompetition(ploeg.Reeks.Competitie);
        //                var frenoySync = new FrenoyApi(dbContent, comp);
        //                var frenoyDivisionId = int.Parse(ploeg.Reeks.LinkId.Substring(0, ploeg.Reeks.LinkId.IndexOf("_")));
        //                frenoyMatchId = frenoySync.GetFrenoyMatchId(frenoyDivisionId, verslag.Week.Value, comp == Competition.Vttl ? "OVL134" : "4055");
        //                if (frenoyMatchId == null)
        //                {
        //                    // WalkOvers?
        //                    reportLostCount++;
        //                    continue;
        //                }
        //            }

        //            var backupRepo = new BackupReport(frenoyMatchId, verslag.Verslag.Beschrijving, verslag.Verslag.SpelerId);
        //            dbContent.BackupReports.Add(backupRepo);
        //        }
        //        dbContent.SaveChanges();
        //        Assert.That(reportLostCount, Is.LessThan(7));
        //    }
        //}

        //[Test]
        //public void BackupTeamPlayers()
        //{
        //    using (var dbContent = new TtcDbContext())
        //    {
        //        var teamPlayers = dbContent.ClubPloegen
        //            .Include(x => x.Reeks)
        //            .Include(x => x.Spelers)
        //            .Where(x => x.ClubId == Constants.OwnClubId);

        //        foreach (ClubPloeg ploeg in teamPlayers)
        //        {
        //            foreach (var player in ploeg.Spelers)
        //            {
        //                var backupPlayer = new BackupTeamPlayer
        //                {
        //                    DivisionLinkId = ploeg.Reeks.LinkId,
        //                    TeamCode = ploeg.Code,
        //                    PlayerId = player.SpelerId.Value
        //                };
        //                dbContent.BackupTeamPlayers.Add(backupPlayer);
        //            }
        //        }
                
        //        dbContent.SaveChanges();
        //    }
        //}
    }
}
