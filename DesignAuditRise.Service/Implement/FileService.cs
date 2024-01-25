using ClosedXML.Excel;
using DesignAuditRise.Service.Interface;
using DesignAuditRise.Web;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOI.HPSF;
using System.Data;
using System.Text;

namespace DesignAuditRise.Service.Implement
{
    public class FileService: IFileService
    {
        /// <summary>
        /// 比對結果下載
        /// </summary>
        /// <param name="tempType"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public async Task<FileStreamResult> ExportExcel(string tempType, DataTable data, string[] header)
        {
            var ms = new MemoryStream();

            XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(tempType);
            int headerRow = 1;// Worksheet.LastRowUsed().RowNumber();

            //Append List_Closed Data
            IXLCell CellForNewData_Closed = ws.Cell(headerRow + 1, 1);
            CellForNewData_Closed.InsertData(data);

            //set header
            for (int i = 0; i < header.Length; i++)
            {
                ws.Cell(headerRow, i + 1).SetValue(header[i]);
            }

            //set column auto width
            ws.Columns().AdjustToContents();

            //set cell style
            var range = ws.Range(ws.Cell(1, 1), ws.Cell(headerRow + data.Rows.Count, header.Length));
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            //輸出Excel報表
            wb.SaveAs(ms);
            ms.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}
