using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using Frenoy.Api;
using NUnit.Framework;
using Ttc.DataAccess;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;
using Ttc.WebApi;

namespace Ttc.UnitTests
{
    [TestFixture]
    public class CreateSportaMatchExcels
    {
        private static string SavePathTemplate => @"c:\temp\ttc-excels\SportaScoresheetTemplate-{season}.xlsx";
        private static string SavePath => @"c:\temp\ttc-excels\";

        /// <summary>
        /// Creates all Sporta home matches excels
        /// </summary>
        [Test]
        public async Task SportaMatchesScoresheetsExcelCreation()
        {
            AutoMapperConfig.Configure(new KlassementValueConverter());

            var service = new MatchService();

            var matches = (await service.GetMatches())
                .Where(x => x.ShouldBePlayed)
                .Where(x => x.Competition == Constants.Sporta)
                .Where(x => x.IsHomeMatch.HasValue && x.IsHomeMatch.Value);

            Directory.CreateDirectory(SavePath);

            foreach (var match in matches)
            {
                var (package, info) = await service.GetExcelExport(match.Id, false);

                var fileName = "{frenoyId} Sporta {teamCode} vs {theirClub} {theirTeam}.xlsx"
                    .Replace("{frenoyId}", info.FrenoyId)
                    .Replace("{teamCode}", info.OurTeamCode)
                    .Replace("{theirClub}", info.TheirTeamName)
                    .Replace("{theirTeam}", info.TheirTeamCode);

                //var dest = SavePath.Replace("{season}", Constants.CurrentSeason.ToString());
                var dir = Path.Combine(SavePath, $"Sporta {info.OurTeamCode}");
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, fileName), package);
            }
        }
    }
}