using System.Drawing;
using System.Globalization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ttc.DataEntities;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Utilities
{
    internal class TeamsExcelCreator
    {
        #region Fields & Constructor
        private readonly ICollection<TeamExcelModel> _model;

        public TeamsExcelCreator(ICollection<TeamExcelModel> model)
        {
            _model = model;
        }
        #endregion

        public byte[] Create()
        {
            using (var package = new ExcelPackage())
            {
                var centerStyle = package.Workbook.Styles.CreateNamedStyle("Center");
                centerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //centerStyle.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //centerStyle.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                //centerStyle.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                //centerStyle.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                foreach (var team in _model)
                {
                    var sheet = package.Workbook.Worksheets.Add(team.Team);
                    int totalColumnCount;
                    var playerToColumnMapping = SetHeader(team, sheet, out totalColumnCount);

                    int rowIndex = 2;
                    foreach (var match in team.Matches)
                    {
                        sheet.Cells[rowIndex, 1].Value = match.Match.FrenoyMatchId;
                        sheet.Cells[rowIndex, 2].Value = DateTimeFormatInfo.CurrentInfo.GetDayName(match.Match.Date.DayOfWeek);
                        sheet.Cells[rowIndex, 3].Value = match.Match.Date.ToString("dd/MM/yyyy");
                        sheet.Cells[rowIndex, 4].Value = match.Match.Date.ToString("HH:mm");
                        if (match.Match.Date.Hour != Constants.DefaultStartHour || match.Match.Date.Minute != 0)
                        {
                            sheet.Cells[rowIndex, 4].Style.Font.Bold = true;
                        }
                        sheet.Cells[rowIndex, 5].Value = match.Home;
                        sheet.Cells[rowIndex, 6].Value = match.Out;

                        if (match.CaptainDecisions.Any())
                        {
                            foreach (var captainDecision in match.CaptainDecisions.Where(x => playerToColumnMapping.ContainsKey(x)))
                            {
                                var cellColumn = playerToColumnMapping[captainDecision];
                                sheet.Cells[rowIndex, cellColumn].Value = "X";
                                sheet.Cells[rowIndex, cellColumn].StyleName = "Center";
                                // Doesn't work on LibreOffice, therefor workaround with StyleName="Center"
                                //sheet.Cells[rowIndex, cellColumn].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                //http://stackoverflow.com/questions/34660560/epplus-isnt-honoring-excelhorizontalalignment-center-or-right
                            }

                            string blockName = match.Match.Block == PlayerMatchStatus.Major ? ExcelExportResources.MatchBlockAdminName : ExcelExportResources.MatchBlockCaptainName;
                            sheet.Cells[rowIndex, totalColumnCount].Value = blockName;
                        }

                        foreach (var playerDecision in match.PlayerDecisions.Where(x => playerToColumnMapping.ContainsKey(x.Key)))
                        {
                            var cellColumn = playerToColumnMapping[playerDecision.Key];
                            var cellColor = GetColor(playerDecision.Value);
                            if (cellColor.HasValue)
                            {
                                sheet.Cells[rowIndex, cellColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                sheet.Cells[rowIndex, cellColumn].Style.Fill.BackgroundColor.SetColor(cellColor.Value);
                            }
                        }

                        rowIndex++;
                    }

                    sheet.Cells.AutoFitColumns();
                }

                return package.GetAsByteArray();
            }
        }

        private static Color? GetColor(string value)
        {
            switch (value)
            {
                case PlayerMatchStatus.Play:
                    return Color.FromArgb(92, 184, 92);
                case PlayerMatchStatus.NotPlay:
                    return Color.FromArgb(217, 83, 79);
                case PlayerMatchStatus.Maybe:
                    return Color.FromArgb(91, 192, 222);
            }
            return null;
        }

        #region Helpers
        private static Dictionary<string, int> SetHeader(TeamExcelModel team, ExcelWorksheet sheet, out int totalHeaderCount)
        {
            var headers = new List<string>() { ExcelExportResources.MatchFrenoyId, ExcelExportResources.MatchDay, ExcelExportResources.MatchDate, ExcelExportResources.MatchHour, ExcelExportResources.MatchHome, ExcelExportResources.MatchOut };
            int baseColumnIndex = headers.Count;

            var players = team.Players.OrderBy(x => x.Reserve).ThenBy(x => x.Name).Select((player, index) => new { Name = player.Name, ColumnIndex = index + baseColumnIndex + 1 }).ToArray();
            headers.AddRange(players.Select(x => x.Name));
            headers.Add(ExcelExportResources.MatchBlock);
            totalHeaderCount = headers.Count;
            var playerNameToColumnIndex = players.ToDictionary(x => x.Name, x => x.ColumnIndex);

            ExcelHelper.SetHeader(sheet, headers.ToArray());

            foreach (var player in team.Players.OrderBy(x => x.Reserve).ThenBy(x => x.Name))
            {
                baseColumnIndex++;
                if (player.Reserve)
                {
                    sheet.Cells[1, baseColumnIndex].Style.Font.Italic = true;
                }
                if (player.Captain)
                {
                    sheet.Cells[1, baseColumnIndex].Style.Font.Color.SetColor(Color.Yellow);
                }
            }

            return playerNameToColumnIndex;
        }
        #endregion



        #region Model Creation
        public static TeamsExcelCreator CreateFormation(TeamEntity[] teams, List<MatchEntity> matches, PlayerEntity[] players, ClubEntity[] clubs)
        {
            var result = new List<TeamExcelModel>();
            foreach (var team in teams.OrderByDescending(x => x.Competition).ThenBy(x => x.TeamCode))
            {
                var teamModel = new TeamExcelModel(team.Competition + " " + team.TeamCode);

                foreach (var teamPlayer in team.Players)
                {
                    var player = players.First(x => x.Id == teamPlayer.PlayerId);
                    teamModel.Players.Add(new TeamPlayerExcelModel(player.NaamKort, teamPlayer.PlayerType));
                }

                foreach (var match in matches.Where(x => x.FrenoyDivisionId == team.FrenoyDivisionId).OrderBy(x => x.Date))
                {
                    if (match.AwayClubId == 0 || match.HomeClubId == 0)
                    {
                        // 'vrije' week
                        continue;
                    }

                    var teamMatch = new TeamMatchExcelModel(match, clubs);
                    foreach (var matchPlayer in match.Players.Where(x => x.Status != PlayerMatchStatus.Captain && x.Status != PlayerMatchStatus.Major))
                    {
                        if (!teamMatch.PlayerDecisions.ContainsKey(matchPlayer.Name))
                        {
                            teamMatch.PlayerDecisions.Add(matchPlayer.Name, matchPlayer.Status);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(match.Block))
                    {
                        foreach (var matchPlayer in match.Players.Where(x => x.Status == match.Block))
                        {
                            if (!teamMatch.CaptainDecisions.Contains(matchPlayer.Name))
                            {
                                teamMatch.CaptainDecisions.Add(matchPlayer.Name);
                            }
                        }
                    }

                    teamModel.Matches.Add(teamMatch);
                }

                result.Add(teamModel);
            }

            return new TeamsExcelCreator(result);
        }
        #endregion
    }

    #region Model Classes
    internal class TeamExcelModel
    {
        public string Team { get; set; }
        public ICollection<TeamPlayerExcelModel> Players { get; set; }
        public ICollection<TeamMatchExcelModel> Matches { get; set; }

        public TeamExcelModel(string team)
        {
            Team = team;
            Matches = new List<TeamMatchExcelModel>();
            Players = new List<TeamPlayerExcelModel>();
        }

        public override string ToString() => $"{Team}";
    }

    internal class TeamPlayerExcelModel
    {
        public string Name { get; set; }
        public bool Reserve { get; set; }
        public bool Captain { get; set; }

        public TeamPlayerExcelModel(string name, TeamPlayerType playerType)
        {
            Name = name;
            Reserve = playerType == TeamPlayerType.Reserve;
            Captain = playerType == TeamPlayerType.Captain;
        }

        public override string ToString() => $"Name={Name}, Reserve={Reserve}";
    }

    internal class TeamMatchExcelModel
    {
        public MatchEntity Match { get; set; }
        public string Home { get; set; }
        public string Out { get; set; }

        /// <summary>
        /// Keys: playerName
        /// Values: Play/NotPlay/Maybe/...
        /// </summary>
        public IDictionary<string, string> PlayerDecisions { get; set; }
        public IList<string> CaptainDecisions { get; set; }

        public TeamMatchExcelModel(MatchEntity match, ClubEntity[] clubs)
        {
            Match = match;
            Home = GetTeamDesc(match.HomeClubId, match.HomeTeamCode, clubs);
            Out = GetTeamDesc(match.AwayClubId, match.AwayTeamCode, clubs);
            PlayerDecisions = new Dictionary<string, string>();
            CaptainDecisions = new List<string>();
        }

        private string GetTeamDesc(int clubId, string teamCode, ClubEntity[] clubs)
        {
            if (clubId == Constants.OwnClubId)
            {
                return "Aalst " + teamCode;
            }
            var club = clubs.Single(x => x.Id == clubId);
            return club.Naam + " " + teamCode;
        }

        public override string ToString() => $"{Match.FrenoyMatchId}";
    }
    #endregion
}