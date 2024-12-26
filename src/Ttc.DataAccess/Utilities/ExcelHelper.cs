using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Ttc.DataAccess.Utilities;

internal static class ExcelHelper
{
    public static void SetHeader(ExcelWorksheet worksheet, params string[] headers)
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
