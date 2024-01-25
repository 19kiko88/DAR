using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;
using DesignAuditRise.Service.Models;
using UniversalLibrary.Models;

namespace DesignAuditRise.Service.Interface
{
    public interface IDesignAuditRiseService
    {
        public string ProtoBuffDataFileName { get; }

        /// <summary>
        /// 從上傳的dsn檔轉檔.exp檔案
        /// </summary>
        /// <param name="dsnFilePath"></param>
        /// <returns></returns>
        public Task<Result> GetExp3FileFromDSN(string dsnFilePath);

        /// <summary>
        /// 從上傳的zip檔轉檔.exp檔案
        /// </summary>
        /// <param name="zipFilePath">zip檔位置</param>
        /// <param name="expFileTempPath">解壓縮後的exp檔案位置</param>
        /// <returns></returns>
        public Task GetExp3FileFromZIP(string zipFilePath, string extractPath);

        /// <summary>
        /// exp1檔案轉換Entity Model
        /// </summary>
        /// <param name="exp1Path"></param>
        /// <returns></returns>
        public Task<Exp1> Exp1FileToEntity(string exp1Path);

        /// <summary>
        /// 篩選出所選取的Schematic Page
        /// </summary>
        /// <param name="originalExp1Entity"></param>
        /// <param name="filterPage"></param>
        /// <returns></returns>
        public Task<Exp1> GetFilterPage(Exp1 originalExp1Entity, string[] filterPage);

        /// <summary>
        /// 線路圖比較
        /// </summary>
        /// <param name="exp1Path1">source exp1 file path</param>
        /// <param name="exp1Path2">destination exp1 file path</param>
        /// <param name="sch1Pages">source selected page</param>
        /// <param name="sch2Pages">destination selected page</param>
        /// <param name="identity1"></param>
        /// <param name="identity2"></param>
        /// <param name="oneWay"></param>
        /// <returns></returns>
        public Task<DesignDiff> DsnCompare(string exp1Path1, string exp1Path2, Exp1 sch1Pages = null, Exp1 sch2Pages = null, bool oneWay = true, string identity1 = "Design1", string identity2 = "Design2");

        /// <summary>
        /// .exp檔案轉檔成.dat檔for Schematic Viewer圖形化使用
        /// </summary>
        /// <param name="expFilePath"></param>
        /// <returns></returns>
        public Task<string> EXPtoData(string expFilePath);

        /// <summary>
        /// UI勾選比對分頁後，篩選part.dat檔案裡的Page，並重新產出part.dat檔 & zip壓縮檔給Schematic Viwer
        /// </summary>
        /// <param name="partDataFilePath">part.dat檔案路徑</param>
        /// <param name="filterPage"></param>
        /// <returns></returns>
        public Task<string> CreateFilterPagePartData(string partDataFilePath, string[] filterPage);

        /// <summary>
        /// 取得Schematic ，並排序分頁(小=>大)
        /// </summary>
        /// <param name="exp1Path">.exp1檔路徑</param>
        /// <returns></returns>
        public Task<(List<SchematicInfoModel> schematics, string errorMsg)> GetSchematicInfo(string exp1Path);
    }
}
