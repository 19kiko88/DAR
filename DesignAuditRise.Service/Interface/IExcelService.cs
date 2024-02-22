using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DesignAuditRise.Service.Interface
{
    public interface IExcelService
    {
        /// <summary>
        /// 比對結果下載
        /// </summary>
        /// <param name="tempType"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public Task<FileStreamResult> ExportExcel(string tempType, DataTable data, string[] header);
    }
}
