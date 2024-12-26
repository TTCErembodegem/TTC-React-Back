using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Matches;

namespace Ttc.DataAccess.Utilities
{
    public class SportaMatchFileInfo
    {
        public string FrenoyId { get; set; }
        public string TheirTeamCode { get; set; }
        public string TheirTeamName { get; set; }
        public string OurTeamCode { get; set; }

        public override string ToString() => $"{FrenoyId} Sporta {OurTeamCode}: {TheirTeamName} {TheirTeamCode}";
    }

    /// <summary>
    /// Create Sporta match scoresheet
    /// </summary>
    internal class SportaMatchExcelCreator
    {
        private readonly ICollection<PlayerEntity> _players;
        private readonly ICollection<TeamEntity> _teams;
        private readonly ICollection<PlayerEntity> _opponentPlayers;
        private readonly ITtcDbContext _context;
        private readonly MatchEntity _match;
        private SportaMatchFileInfo _fileInfo;

        private static string TemplatePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\SportaScoresheetTemplate.xlsx");

        /// <summary>
        /// ATTN: Set after create!
        /// </summary>
        public SportaMatchFileInfo FileInfo => _fileInfo;

        public SportaMatchExcelCreator(
            ITtcDbContext context,
            MatchEntity match,
            ICollection<PlayerEntity> players,
            ICollection<TeamEntity> teams,
            ICollection<PlayerEntity> opponentPlayers
        )
        {
            _context = context;
            _match = match;
            _players = players;
            _teams = teams;
            _opponentPlayers = opponentPlayers;
        }

        public byte[] Create(bool fillInOurTeam = true)
        {
            var template = new FileInfo(TemplatePath);
            using (var package = new ExcelPackage(template))
            {
                // Update formula's etc
                CreateTemplatePlayers(package);
                CreateTemplateVisitors(package);
                CreateTemplateVisitorPlayers(package);

                var scoresheet = package.Workbook.Worksheets["Wedstrijdblad"];
                var location = _context.Parameters.First(p => p.Sleutel == "location");
                scoresheet.Cells["B5"].Value = location.Value;

                // Fill in match details
                scoresheet.Cells["B6"].Value = _match.Date;
                scoresheet.Cells["G6"].Value = _match.Date.ToString(@"HH\umm");
                scoresheet.Cells["U4"].Value = _match.FrenoyMatchId;

                var ourTeam = _match.HomeTeam ?? _match.AwayTeam;
                scoresheet.Cells["W5"].Value = string.IsNullOrWhiteSpace(ourTeam.ReeksNummer) ? "Ere" : ourTeam.ReeksNummer + ourTeam.ReeksCode;
                scoresheet.Cells["A9"].Value = $"Aalst {ourTeam.TeamCode}";

                var theirTeam = GetTheirTeam();
                scoresheet.Cells["A16"].Value = theirTeam.ClubName + " " + theirTeam.TeamCode;

                _fileInfo = new SportaMatchFileInfo()
                {
                    FrenoyId = _match.FrenoyMatchId.Replace("/", "-"),
                    OurTeamCode = ourTeam.TeamCode,
                    TheirTeamName = theirTeam.ClubName,
                    TheirTeamCode = theirTeam.TeamCode,
                };

                if (fillInOurTeam)
                {
                    int playerIndex = 10;

                    // TODO: hier kunnen alle spelers meerdere keren inzitten....
                    // --> checken of er wel altijd een Major is?

                    // OrderBy(SportaKlassementn,j ).ThenBy(Postion)

                    foreach (var player in _match.Players.Where(x => x.Player != null).Where(x => x.Status == PlayerMatchStatus.Major).OrderBy(x => x.Player.IndexSporta))
                    {
                        scoresheet.Cells["B" + playerIndex].Value = player.Player.Name;
                        playerIndex++;
                    }
                }

                package.Workbook.Worksheets["Ploegen"].Hidden = eWorkSheetHidden.Hidden;
                package.Workbook.Worksheets["Spelers"].Hidden = eWorkSheetHidden.Hidden;

                scoresheet.Calculate();

                return package.GetAsByteArray();
            }
        }

        private (string ClubName, string TeamCode) GetTheirTeam()
        {
            var theirClubId = _match.HomeTeamId.HasValue ? _match.AwayClubId : _match.HomeClubId;
            var club = _context.Clubs.Single(x => x.Id == theirClubId);
            if (_match.HomeTeamId.HasValue)
            {
                return (club.Naam, _match.AwayTeamCode);
            }
            return (club.Naam, _match.HomeTeamCode);
        }

        private string GetTheirTeamDesc()
        {
            var theirTeam = GetTheirTeam();
            return theirTeam.ClubName + " " + theirTeam.TeamCode;
        }

        #region Template
        private void CreateTemplateVisitorPlayers(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets["Tegenstanders"];
            //sheet.Column(1).Width = 20;
            sheet.Cells["A1"].Value = GetTheirTeamDesc();

            //sheet.Cells["A2"].Value = "Naam";
            //sheet.Cells["B2"].Value = "Lidkaart";
            //sheet.Cells["C2"].Value = "Klassement";
            //sheet.Cells["D2"].Value = "Waarde";
            int rowIndex = 3;
            foreach (var opponent in _opponentPlayers.OrderBy(x => x.Name))
            {
                sheet.Cells["A" + rowIndex].Value = opponent.Name;
                sheet.Cells["B" + rowIndex].Value = opponent.LidNummerSporta;
                sheet.Cells["C" + rowIndex].Value = opponent.KlassementSporta;
                sheet.Cells["D" + rowIndex].Value = KlassementValueConverter.Sporta(opponent.KlassementSporta);
                rowIndex++;
            }

            var spelersRange = package.Workbook.Names["tegenstanders"];
            spelersRange.Address = "'Tegenstanders'!$A$3:$A$" + rowIndex;
        }

        private void CreateTemplateVisitors(ExcelPackage package)
        {
            var ploegenSheet = package.Workbook.Worksheets["Ploegen"];

            var teams = _teams.OrderBy(x => x.TeamCode).ToArray();
            var ploegen = teams
                .Select(x => new
                {
                    Ploegen = $"Aalst {x.TeamCode}",
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
            var playersSheet = package.Workbook.Worksheets["Spelers"];

            var players = _players
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    Naam = x.Name,
                    Klassement = KlassementValueConverter.Sporta(x.KlassementSporta),
                    Waarde = x.KlassementSporta,
                    Lidkaart = x.LidNummerSporta
                })
                .ToArray();

            playersSheet.Cells["A2"].LoadFromCollection(players);

            int lastPlayerIndex = players.Length + 1;

            var spelersRange = package.Workbook.Names["spelers"];
            spelersRange.Address = "'Spelers'!$A$2:$A$" + lastPlayerIndex;

            // TODO: This didn't work. The template has been updated to 'Spelers'!A1:E50 (so as long as we don't get 50 Sporta members, all is good:)
            //playersSheet.Cells["D10:D12"].Formula = $"IF(ISERROR(VLOOKUP(B10,'Spelers'!A1:E{lastPlayerIndex},4))=TRUE,\"\",VLOOKUP(B10,'Spelers'!A1:E{lastPlayerIndex},4))";
            //var e10 = playersSheet.Cells["E10"];
            //e10.Formula = $"IF(ISERROR(VLOOKUP(B10,'Spelers'!A1:E{lastPlayerIndex},4))=TRUE,\"\",VLOOKUP(B10,'Spelers'!A1:E{lastPlayerIndex},3))";
            //playersSheet.Cells["F10"].Formula = $"IF(ISERROR(VLOOKUP(B10,'Spelers'!A1:E{lastPlayerIndex},4))=TRUE,\"\",VLOOKUP(B10,'Spelers'!A1:E{lastPlayerIndex},2))";
        }
        #endregion
    }
}