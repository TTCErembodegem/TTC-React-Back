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
    /// Create Sporta match scoresheet
    /// </summary>
    internal class SportaMatchExcelCreator
    {
        private readonly ICollection<PlayerEntity> _players;
        private readonly ICollection<TeamEntity> _teams;
        private readonly Match _match;

        private static string TemplatePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\SportaScoresheetTemplate.xlsx");

        public SportaMatchExcelCreator(Match match, ICollection<PlayerEntity> players, ICollection<TeamEntity> teams)
        {
            _match = match;
            _players = players;
            _teams = teams;
        }

        public byte[] Create()
        {
            var template = new FileInfo(TemplatePath);
            using (var package = new ExcelPackage(template))
            {
                // Update formula's etc
                CreateTemplatePlayers(package);
                CreateTemplateVisitors(package);

                // Fill in match details
                var scoresheet = package.Workbook.Worksheets["Wedstrijdblad"];
                scoresheet.Cells["B6"].Value = _match.Date;
                scoresheet.Cells["G6"].Value = _match.Date.ToString(@"HH\umm");
                scoresheet.Cells["U4"].Value = _match.FrenoyMatchId;

                var team = _teams.Single(x => x.Id == _match.TeamId);
                scoresheet.Cells["W5"].Value = string.IsNullOrWhiteSpace(team.ReeksNummer) ? "Ere" : team.ReeksNummer + team.ReeksCode;



                scoresheet.Cells["A9"].Value = $"Erembodegem {team.TeamCode}";
                //scoresheet.Cells["A16"].Value = $"{_match.Opponent.}";

                return package.GetAsByteArray();
            }
        }

        #region Template
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

    }
}