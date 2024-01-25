using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;
using DesignAuditRise.Service.Interface;
using DesignAuditRise.Service.Models;
using DesignAuditRise.Service.Utility;
using DsnLibrary.Models;
using ICSharpCode.SharpZipLib.Zip;
using ProtoBuf;
using UniversalLibrary.Models;

namespace DesignAuditRise.Service.Implement
{
    public class DesignAuditRiseService : IDesignAuditRiseService
    {
        private readonly OuterService.Interface.IConvertPediaOuterService _outerConvertPediaService;
        private readonly OuterService.Interface.IDesignAuditRiseOuterService _designAuditRiseService;
        private readonly string[] allowFileNameFromExpFileFolder = { "part.dat", "cache.dat", "font.dat", "tree.dat", ".png", ".pdf" };
        private readonly string[] allowFileNameFromUploadZip = { "part.exp", "cache.exp", "font.exp", ".exp1", ".png", ".pdf" };
        public string ProtoBuffDataFileName { get; } = "ProtoBuffData.zip";

        public DesignAuditRiseService(
            OuterService.Interface.IConvertPediaOuterService convertPediaService,
            OuterService.Interface.IDesignAuditRiseOuterService designAuditRiseService
        )
        {
            _outerConvertPediaService = convertPediaService;
            _designAuditRiseService = designAuditRiseService;
        }

        /// <summary>
        /// 從上傳的dsn檔轉檔.exp檔案
        /// </summary>
        /// <param name="dsnFilePath">dsn檔位置</param>
        /// <returns></returns>
        public async Task<Result> GetExp3FileFromDSN(string dsnFilePath)
        {
            return _outerConvertPediaService.ToExp3File(dsnFilePath).Result;
        }

        /// <summary>
        /// 從上傳的zip檔轉檔.exp檔案
        /// </summary>
        /// <param name="zipFilePath">zip檔位置</param>
        /// <param name="expFileTempPath">解壓縮後的exp檔案位置</param>
        /// <returns></returns>
        public async Task GetExp3FileFromZIP(string zipFilePath, string expFileTempPath)
        {
            var filterFiles = new string[] { };
            var fastZip = new FastZip();

            //建立暫存解壓縮資料的資料夾
            var extractPath = zipFilePath.Replace(Path.GetExtension(zipFilePath), "");
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            //解壓縮
            fastZip.ExtractZip(zipFilePath, extractPath, null);

            //篩選檔名part.exp, cache.exp, font.exp & 副檔名為.png, .pdf的壓縮資料，搬移解壓縮檔案到exp file資料夾
            filterFiles = Directory.GetFiles(extractPath, "*", SearchOption.AllDirectories).Where(p => allowFileNameFromUploadZip.Any(q => p.EndsWith(q, StringComparison.OrdinalIgnoreCase))).ToArray();

            //搬移解壓縮檔案到exp file資料夾
            foreach (var f in filterFiles)
            {
                File.Copy(f, Utils.SecurityPathCombine(expFileTempPath, Path.GetFileName(f)), true);
            }

            //刪除暫存解壓縮資料的資料夾            
            Directory.Delete(extractPath, true);

        }

        /// <summary>
        /// exp1檔案轉換Entity Model
        /// </summary>
        /// <param name="exp1Path"></param>
        /// <returns></returns>
        public async Task<Exp1> Exp1FileToEntity(string exp1Path)
        {
            var result = new Exp1();

            try
            {
                var res = await _outerConvertPediaService.Exp1FileToEntity(exp1Path);
                result = res.Content;
            }
            catch (Exception ex)
            {
                result = null;
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// 篩選出所選取的Schematic Page
        /// </summary>
        /// <param name="originalExp1Entity"></param>
        /// <param name="filterPage"></param>
        /// <returns></returns>
        public async Task<Exp1> GetFilterPage(Exp1 originalExp1Entity, string[] filterPage)
        {
            var exp1Entity = originalExp1Entity;
            var cloneExp1Schematics = originalExp1Entity.Schematics.Select(c => new Schematic { Name = c.Name, Pages = c.Pages }).ToList();
            var cloneExp1Entity = new Exp1()
            {
                Path = exp1Entity.Path,
                Schematics = cloneExp1Schematics
            };
            foreach (var schematicItem in cloneExp1Entity.Schematics)
            {
                var filterPages = schematicItem.Pages.Where(c => filterPage.Contains(c.Name)).ToList();
                schematicItem.Pages = filterPages;
            }

            return cloneExp1Entity;
        }

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
        public async Task<DesignDiff> DsnCompare(string exp1Path1, string exp1Path2, Exp1 sch1Pages = null, Exp1 sch2Pages = null, bool oneWay = true, string identity1 = "Design1", string identity2 = "Design2")
        {
            var res = new DesignDiff();
            var diffData = _designAuditRiseService.SkrDsnCompare(exp1Path1, exp1Path2, identity1, identity2, sch1Pages, sch2Pages, oneWay);
            if (diffData.Success)
            {
                res.PartDiffs = diffData.Content.PartDiffs;
                res.NetDiffs = diffData.Content.NetDiffs;
                res.Design1_2 = diffData.Content.Design1_2;
                res.Design2_1 = diffData.Content.Design2_1;
                res.Identity1 = diffData.Content.Identity1;
                res.Identity2 = diffData.Content.Identity2;

                return res;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// .exp檔案轉檔成.dat檔for Schematic Viewer圖形化使用
        /// </summary>
        /// <param name="expFilePath"></param>
        /// <returns></returns>
        public async Task<string> EXPtoData(string expFilePath)
        {
            var errMsg = string.Empty;
            var processDsn = new DsnLibrary.ProcessDSN(expFilePath);

            if (processDsn.EXPtoData("", out errMsg))
            {
                try
                {
                    //儲存成維修檔                    
                    string[] schematicViwerFiles = Directory.EnumerateFiles(expFilePath).Where(p => allowFileNameFromExpFileFolder.Any(q => p.EndsWith(q, StringComparison.OrdinalIgnoreCase))).ToArray();
                    FileProcessUtils.zipFiles(expFilePath + "\\" + ProtoBuffDataFileName, schematicViwerFiles);
                }
                catch (Exception ex)
                {
                    errMsg = ex.Message;
                }
            }

            return errMsg;
        }

        /// <summary>
        /// UI勾選比對分頁後，篩選part.dat檔案裡的Page，並重新產出filter_part.dat檔 & zip壓縮檔給Schematic Viwer
        /// </summary>
        /// <param name="partDataFilePath">part.dat檔案路徑</param>
        /// <param name="filterPage"></param>
        /// <returns></returns>
        public async Task<string> CreateFilterPagePartData(string partDataFilePath, string[] filterPage) 
        {
            var errMsg = string.Empty;

            try
            {
                if (File.Exists(partDataFilePath))
                {
                    var bytes = File.ReadAllBytes(partDataFilePath);
                    var partData = new List<PagePart>();

                    //把原本的dat反序列化回Entity，並篩選分頁.
                    using (var ms = new MemoryStream(bytes, 0, bytes.Length, true, true))
                    {
                        partData = FileProcessUtils.LoadStream<PagePart>(ms);
                    }
                    partData = partData.Where(c => filterPage.Contains(c.Page)).ToList();

                    //刪除原本的filter_part.dat檔
                    var filterDatFilePath = Utils.SecurityPathCombine(Path.GetDirectoryName(partDataFilePath), "filter_part.dat");
                    if (File.Exists(filterDatFilePath))
                    {
                        File.Delete(filterDatFilePath);
                    }
                    var filterPartData = File.Create(filterDatFilePath);

                    //序列化回新的filter_part.dat檔
                    Serializer.Serialize((Stream)filterPartData, partData);

                    filterPartData.Close();

                    //重新壓縮zip檔(要給Schematic Viewer的檔案)
                    var parentPath = partDataFilePath.Substring(0, partDataFilePath.LastIndexOf(@"\") + 1);
                    var zipFiles = Directory.EnumerateFiles(parentPath).Where(p => allowFileNameFromExpFileFolder.Any(q => p.EndsWith(q, StringComparison.OrdinalIgnoreCase))).ToArray();
                    FileProcessUtils.zipFiles(Utils.SecurityPathCombine(parentPath, ProtoBuffDataFileName), zipFiles);
                }
                else
                {
                    errMsg = "查無part.dat檔案.";
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return errMsg;
        }

        /// <summary>
        /// 取得Schematic ，並排序分頁(小=>大)
        /// </summary>
        /// <param name="exp1Path">.exp1檔路徑</param>
        /// <returns></returns>
        public async Task<(List<SchematicInfoModel> schematics, string errorMsg)> GetSchematicInfo(string exp1Path)
        {
            var res = new List<SchematicInfoModel>();
            var msg = string.Empty;

            var exp1Entity = Exp1FileToEntity(exp1Path).Result;

            if (exp1Entity.Schematics.Count == 0)
            {
                msg = "exp1檔案查無Schematics相關訊息.";
            }

            foreach (var itemSchematic in exp1Entity.Schematics)
            {
                var schematic = new SchematicInfoModel()
                {
                    item = itemSchematic.Name,
                    Children = new List<SchematicPageInfoModel>()
                };

                foreach (var page in itemSchematic.Pages)
                {
                    var child = new SchematicPageInfoModel();
                    child.item = page.Name;
                    child.level = 2;
                    schematic.Children.Add(child);
                }

                res.Add(schematic);
            }

            foreach (var item in res)
            {
                item.Children = item.Children.OrderBy(c => c.item).ToList();
            }

            return (res, msg);
        }
    }
}
