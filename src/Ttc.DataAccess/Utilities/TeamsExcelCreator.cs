using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
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
                foreach (var team in _model)
                {
                    var sheet = package.Workbook.Worksheets.Add(team.Team);
                    var playerToColumnMapping = SetHeader(team, sheet);

                    int rowIndex = 2;
                    foreach (var match in team.Matches)
                    {
                        sheet.Cells[rowIndex, 1].Value = match.Match.FrenoyMatchId;
                        sheet.Cells[rowIndex, 2].Value = DateTimeFormatInfo.CurrentInfo.GetDayName(match.Match.Date.DayOfWeek);
                        sheet.Cells[rowIndex, 3].Value = match.Match.Date.ToString("dd/MM/yyyy");
                        sheet.Cells[rowIndex, 4].Value = match.Match.Date.ToString("HH:mm");
                        sheet.Cells[rowIndex, 5].Value = match.Home;
                        sheet.Cells[rowIndex, 6].Value = match.Out;

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
        private static Dictionary<string, int> SetHeader(TeamExcelModel team, ExcelWorksheet sheet)
        {
            var headers = new List<string>() { ExcelExportResources.MatchFrenoyId, ExcelExportResources.MatchDay, ExcelExportResources.MatchDate, ExcelExportResources.MatchHour, ExcelExportResources.MatchHome, ExcelExportResources.MatchOut };
            int baseColumnIndex = headers.Count;

            var players = team.Players.OrderBy(x => x.Reserve).ThenBy(x => x.Name).Select((player, index) => new { Name = player.Name, ColumnIndex = index + baseColumnIndex + 1 }).ToArray();
            headers.AddRange(players.Select(x => x.Name));
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
                    var teamMatch = new TeamMatchExcelModel(match, clubs);
                    foreach (var matchPlayer in match.Players.Where(x => x.Status != PlayerMatchStatus.Captain && x.Status != PlayerMatchStatus.Major))
                    {
                        if (!teamMatch.PlayerDecisions.ContainsKey(matchPlayer.Name))
                        {
                            teamMatch.PlayerDecisions.Add(matchPlayer.Name, matchPlayer.Status);
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

        public TeamMatchExcelModel(MatchEntity match, ClubEntity[] clubs)
        {
            Match = match;
            Home = GetTeamDesc(match.HomeClubId, match.HomeTeamCode, clubs);
            Out = GetTeamDesc(match.AwayClubId, match.AwayTeamCode, clubs);
            PlayerDecisions = new Dictionary<string, string>();
        }

        private string GetTeamDesc(int clubId, string teamCode, ClubEntity[] clubs)
        {
            if (clubId == Constants.OwnClubId)
            {
                return "Erembodegem " + teamCode;
            }
            var club = clubs.Single(x => x.Id == clubId);
            return club.Naam + " " + teamCode;
        }

        public override string ToString() => $"{Match.FrenoyMatchId}";
    }
    #endregion
}