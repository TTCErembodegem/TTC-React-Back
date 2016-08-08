using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ttc.DataEntities;

namespace Ttc.DataAccess.Utilities
{
    public class PlayerExcelCreator
    {
        private readonly ICollection<PlayerEntity> _players;

        public PlayerExcelCreator(ICollection<PlayerEntity> players)
        {
            _players = players;
        }

        public byte[] Create()
        {
            //using (var package = new ExcelPackage(new FileInfo(@"c:\temp\ttc-excels\testy-" + DateTime.Now.ToString("yyyy-M-d HH.mm.ss") + ".xlsx")))
            using (var package = new ExcelPackage())
            {
                var allPlayerSheet = package.Workbook.Worksheets.Add(PlayerExcelExportResources.SheetAllPlayers);
                SetHeader(
                    allPlayerSheet,
                    PlayerExcelExportResources.PlayerName,
                    PlayerExcelExportResources.PlayerAddress,
                    PlayerExcelExportResources.PlayerCity,
                    PlayerExcelExportResources.PlayerPhone,
                    PlayerExcelExportResources.PlayerEmail
                );

                SetPlayersSheet(allPlayerSheet);
                allPlayerSheet.Cells.AutoFitColumns();
                allPlayerSheet.Column(5).Width = 50;

                var vttlSheet = package.Workbook.Worksheets.Add(PlayerExcelExportResources.SheetVttl);
                SetVttlSheet(vttlSheet);

                var sportaSheet = package.Workbook.Worksheets.Add(PlayerExcelExportResources.SheetSporta);
                SetSportaSheet(sportaSheet);

                //package.Save(); // save to file

                return package.GetAsByteArray();
            }
        }

        private void SetSportaSheet(ExcelWorksheet worksheet)
        {
            SetHeader(
                worksheet,
                PlayerExcelExportResources.CompetitionVolgnummer,
                PlayerExcelExportResources.CompetitionIndex,
                PlayerExcelExportResources.CompetitionLidnummer,
                PlayerExcelExportResources.PlayerName,
                PlayerExcelExportResources.PlayerRanking,
                PlayerExcelExportResources.PlayerRankingValue
            );

            int i = 2;
            var rankingValueConverter = new KlassementValueConverter();
            foreach (var player in _players.Where(x => x.ClubIdSporta == Constants.OwnClubId).OrderBy(x => x.VolgnummerSporta))
            {
                worksheet.Cells[i, 1].Value = player.VolgnummerSporta;
                worksheet.Cells[i, 2].Value = player.IndexSporta;
                worksheet.Cells[i, 3].Value = player.LidNummerSporta;
                worksheet.Cells[i, 4].Value = player.Naam;
                worksheet.Cells[i, 5].Value = player.KlassementSporta;
                worksheet.Cells[i, 6].Value = rankingValueConverter.Sporta(player.KlassementSporta);

                i++;
            }
            worksheet.Cells.AutoFitColumns();
        }

        private void SetVttlSheet(ExcelWorksheet worksheet)
        {
            SetHeader(
                worksheet,
                PlayerExcelExportResources.CompetitionVolgnummer,
                PlayerExcelExportResources.CompetitionIndex,
                PlayerExcelExportResources.CompetitionComputerNumber,
                PlayerExcelExportResources.PlayerName,
                PlayerExcelExportResources.PlayerRanking
            );

            int i = 2;
            foreach (var player in _players.Where(x => x.ClubIdVttl == Constants.OwnClubId).OrderBy(x => x.VolgnummerVttl))
            {
                worksheet.Cells[i, 1].Value = player.VolgnummerVttl;
                worksheet.Cells[i, 2].Value = player.IndexVttl;
                worksheet.Cells[i, 3].Value = player.ComputerNummerVttl;
                worksheet.Cells[i, 4].Value = player.Naam;
                worksheet.Cells[i, 5].Value = player.KlassementVttl;

                i++;
            }
            worksheet.Cells.AutoFitColumns();
        }

        private void SetPlayersSheet(ExcelWorksheet worksheet)
        {
            int i = 2;
            foreach (var player in _players.OrderBy(x => x.Naam))
            {
                worksheet.Cells[i, 1].Value = player.Naam;
                worksheet.Cells[i, 2].Value = player.Adres;
                worksheet.Cells[i, 3].Value = player.Gemeente;

                var telephoneCell = worksheet.Cells[i, 4];
                int telephone;
                if (int.TryParse(player.Gsm, out telephone))
                {
                    telephoneCell.Value = $"{telephone:####/## ## ##}";
                }
                else
                {
                    telephoneCell.Value = player.Gsm;
                }

                if (!string.IsNullOrWhiteSpace(player.Email))
                {
                    worksheet.Cells[i, 5].Formula = "HYPERLINK(\"mailto:" + player.Email + "\",\"" + player.Email + "\")";
                    worksheet.Cells[i, 5].Style.Font.Color.SetColor(Color.Blue);
                    worksheet.Cells[i, 5].Style.Font.UnderLine = true;
                }
                i++;
            }
        }

        private static void SetHeader(ExcelWorksheet worksheet, params string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                cell.Style.Font.Color.SetColor(Color.White);
            }
        }
    }
}