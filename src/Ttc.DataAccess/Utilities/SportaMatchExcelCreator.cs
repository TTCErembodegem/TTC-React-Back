using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ttc.DataEntities;
using Ttc.Model.Matches;

namespace Ttc.DataAccess.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    internal class SportaMatchExcelCreator
    {
        private readonly ICollection<PlayerEntity> _players;
        private readonly ICollection<TeamEntity> _teams;
        private readonly Match _match;

        private static string TemplatePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\SportaScoresheetTemplate.xlsx");
        private static string SavePath => @"c:\temp\ttc-excels\SportaScoresheetTemplate-{season}.xlsx";


        public SportaMatchExcelCreator(Match match, ICollection<PlayerEntity> players, ICollection<TeamEntity> teams)
        {
            _match = match;
            _players = players;
            _teams = teams;
        }

        #region Template
        public byte[] CreateTemplate()
        {
            var template = new FileInfo(TemplatePath);
            var dest = new FileInfo(SavePath.Replace("{season", Constants.CurrentSeason.ToString()));
            //var dest = new FileInfo(@"c:\temp\ttc-excels\testy-" + DateTime.Now.ToString("yyyy-M-d HH.mm.ss") + ".xlsx");

            using (var package = new ExcelPackage(dest, template))
            {
                CreateTemplatePlayers(package);
                CreateTemplateVisitors(package);

                return package.GetAsByteArray();
            }
        }

        private void CreateTemplateVisitors(ExcelPackage package)
        {
            var ploegenSheet = package.Workbook.Worksheets["Ploegen"];

            var teams = _teams.OrderBy(x => x.TeamCode).ToArray();
            var ploegen = teams
                .Select(x => new
                {
                    Ploegen = $"Erembodegem {x.TeamCode}",
                    Afdeling = string.IsNullOrWhiteSpace(x.ReeksNummer) ? "Ere" : x.ReeksNummer,
                    Reeks = x.ReeksCode,
                })
                .ToArray();

            ploegenSheet.Cells["A2"].LoadFromCollection(ploegen);

            var ploegenRange = package.Workbook.Names["Ploegen"];
            ploegenRange.Address = "'Ploegen'!$A$2:$A$" + (ploegen.Length + 1);

            int i = ploegen.Length + 3;
            foreach (var team in teams)
            {
                if (team.TeamCode == "G")
                    Debug.Fail("Not available in the Excel... But can just as easily be added in code?");

                ploegenSheet.Cells[i, 1].Value = $"Bezoekers {team.TeamCode}-ploeg";
                var opponents = team.Opponents
                    .Select(x => x.Club.Naam + " " + x.TeamCode)
                    .OrderBy(x => x)
                    .ToArray();

                ploegenSheet.Cells[i + 1, 1].LoadFromCollection(opponents);

                ploegenRange = package.Workbook.Names["bezoekers" + team.TeamCode];
                ploegenRange.Address = "'Ploegen'!$A$" + (i + 1) + ":$A$" + (i + opponents.Length);

                i += opponents.Length + 2;
            }
        }

        private void CreateTemplatePlayers(ExcelPackage package)
        {
            var klassementCalc = new KlassementValueConverter();
            var playersSheet = package.Workbook.Worksheets["Spelers"];

            var players = _players
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    Naam = x.Name,
                    Waarde = x.KlassementSporta,
                    Klassement = klassementCalc.Sporta(x.KlassementSporta),
                    Lidkaart = x.LidNummerSporta
                })
                .ToArray();

            playersSheet.Cells["A2"].LoadFromCollection(players);


            var spelersRange = package.Workbook.Names["spelers"];
            spelersRange.Address = "'Spelers'!$A$2:$A$" + (players.Length + 1);
        }
        #endregion


        public byte[] Create()
        {
            var dest = new FileInfo(@"c:\temp\ttc-excels\testy-" + DateTime.Now.ToString("yyyy-M-d HH.mm.ss") + ".xlsx");
            var template = new FileInfo(@"C:\Users\Wouter\Desktop\SportaScoresheet.xlsx");

            using (var package = new ExcelPackage(dest, template))
            {
                var scoresheet = package.Workbook.Worksheets["Wedstrijdblad"];
                scoresheet.Cells["B6"].Value = _match.Date;
                return package.GetAsByteArray();
            }
        }
    }
}