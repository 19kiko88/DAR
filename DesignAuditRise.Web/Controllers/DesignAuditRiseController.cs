using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalLibrary.Models;
using Microsoft.Extensions.Options;
using System.Data;

using DesignAuditRise.Service.Interface;
using DesignAuditRise.Web.Dtos.Response;
using DesignAuditRise.Web.Models;
using DesignAuditRise.Service.Utility;
using DesignAuditRise.Web.Enums;
using DesignAuditPedia.Models.Dsn.DesignCompare;
using DesignAuditRise.Service.Models;
using System.Text.RegularExpressions;
using DesignAuditRise.Web.Filters;

namespace DesignAuditRise.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DesignAuditRiseController : ControllerBase
    {
        private readonly IDesignAuditRiseService _designAuditRiseService;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly IExcelService _excelService;
        private string dsnFileUploadPath = string.Empty;
        private string zipFileUploadPath = string.Empty;
        private string expFileTempPath = string.Empty;
        private string datFileTempPath = string.Empty;
        private string[] DsnAllias = new string[2];
        private string webSiteUrl = string.Empty;
        private static Dictionary<string, object> lockDic = new Dictionary<string, object>();
        private readonly object lockObj = new object ();

        /*
        *172.21.130.8 => prd
        *172.22.136.54 => vt01
        *172.22.136.97 => vt02
        */
        //private const string _allowIpList = "172.21.130.8,172.22.136.54,172.22.136.97,::1,127.0.0.1";

        public DesignAuditRiseController(
            IDesignAuditRiseService designAuditRiseService,
            IWebHostEnvironment hostEnv,
            IExcelService excelService,
            IOptions<AppSettingsInfoModel.PathSettings> pathSettings,
            IOptions<AppSettingsInfoModel.OtherSettings> otherSettings,
            IOptions<AppSettingsInfoModel.UrlSettings> urlSettings
        )
        { 
            _designAuditRiseService = designAuditRiseService;
            _hostEnv = hostEnv;
            _excelService = excelService;
            dsnFileUploadPath = Utils.SecurityPathCombine(hostEnv.ContentRootPath, "Content\\Upload\\Dsn");
            zipFileUploadPath = Utils.SecurityPathCombine(hostEnv.ContentRootPath, "Content\\Upload\\Zip");
            expFileTempPath = Utils.SecurityPathCombine(hostEnv.ContentRootPath, "Content\\ExpFile");
            datFileTempPath = Utils.SecurityPathCombine(hostEnv.ContentRootPath, pathSettings.Value?.DatFileTempPath);
            DsnAllias = otherSettings.Value?.DsnAlias;
            webSiteUrl = urlSettings.Value?.webSite;
        }

        /// <summary>
        /// 檔案上傳
        /// Ref：https://arminzia.com/blog/file-upload-with-aspnet-core-and-angular/
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Result<ResUploadFileInfoDto>> Upload()
        {
            var result = new Result<ResUploadFileInfoDto>();

            try
            {
                if (!Directory.Exists(dsnFileUploadPath))
                {
                    Directory.CreateDirectory(dsnFileUploadPath);
                }
                if (!Directory.Exists(zipFileUploadPath))
                {
                    Directory.CreateDirectory(zipFileUploadPath);
                }
                
                var uploadFileInfo = new ResUploadFileInfoDto();
                var file = Request.Form.Files[0];
                var fileName = file.FileName;

                if (!Regex.IsMatch(file.FileName, @"((?i)(zip|dsn)$)"))
                {
                    throw new Exception("檔案名稱副檔名必須為zip, dsn.");
                }
                if (!Regex.IsMatch(file.FileName, @"(^([a-zA-Z0-9\-_()]+\.(?i)(zip|dsn))$)"))
                {
                    fileName = Guid.NewGuid().ToString();
                }

                var extension = Path.GetExtension(file.FileName).ToLower();
                uploadFileInfo.OldFileName = file.FileName;                
                uploadFileInfo.NewFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{extension}";

                var fullPath = Utils.SecurityPathCombine(extension == ".dsn" ? dsnFileUploadPath : zipFileUploadPath, uploadFileInfo.NewFileName);
                var buffer = 1024 * 1024;
                using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer, useAsync: false);
                await file.CopyToAsync(stream);
                await stream.FlushAsync();

                var fileInfo = new FileInfo(fullPath);
                uploadFileInfo.Size = fileInfo.Length / 1024;//KB

                result.Content = uploadFileInfo;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 從上傳的dsn或zip檔轉檔.exp檔案
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Result<ResToExp3FileDto>> ToExp3File([FromBody] Dtos.Request.ReqToExp3FileDto data)
        {
            var result = new Result<ResToExp3FileDto>() { };
            var userName = User.Identity.Name.Split('\\')[1];

            try
            {
                var remoteExp1 = false;
                var extension = Path.GetExtension(data.UploadFileName).ToLower();
                var expFilePath = string.Empty;
                var fullFileUploadPath = Utils.SecurityPathCombine(extension == ".dsn" ? dsnFileUploadPath : zipFileUploadPath, data.UploadFileName);
                var fullExpFileTempPath = Utils.SecurityPathCombine(expFileTempPath, userName, data.FileType == FileType.Source ? "Source" : "Destination");
                var fullDatFileTempPath = Utils.SecurityPathCombine(datFileTempPath, userName, data.FileType == FileType.Source ? "Source" : "Destination");

                if (!lockDic.ContainsKey(userName))
                {
                    lockDic.Add(userName, new object());
                }

                lock (lockDic[userName])//get lock object by different userName
                {
                    #region lock測試
                    //if (userName.ToLower() == "homer_chen")
                    //{
                    //    Thread.Sleep(30000);
                    //}
                    #endregion

                    if (Directory.Exists(fullExpFileTempPath))
                    {
                        Directory.Delete(fullExpFileTempPath, true);
                    }
                    Directory.CreateDirectory(fullExpFileTempPath);

                    if (Directory.Exists(fullDatFileTempPath))
                    {
                        Directory.Delete(fullDatFileTempPath, true);
                    }
                    Directory.CreateDirectory(fullDatFileTempPath);

                    if (extension == ".dsn")
                    {
                        //取得server的exp檔案資料夾
                        var res = _designAuditRiseService.GetExp3FileFromDSN(fullFileUploadPath).Result;


                        if (res.Success)
                        {
                            expFilePath = res.Message;
                            try
                            {
                                //搬移exp檔案到本機資料夾
                                foreach (var f in Directory.GetFiles(expFilePath))
                                {
                                    var fileName = Path.GetFileName(f);
                                    System.IO.File.Copy(f, Utils.SecurityPathCombine(fullExpFileTempPath, fileName), true);
                                }

                                expFilePath = fullExpFileTempPath;//遠端server路徑
                            }
                            catch (Exception ex)
                            {
                                remoteExp1 = true;//沒有遠端資料夾權限，不複製exp檔案回本機。直接使用遠端資料
                                result.Message = $"遠端.exp & .exp1檔案搬移失敗。{ex.Message}.";
                                return result;
                            }
                        }
                        else
                        {
                            result.Message = res.Message;
                            return result;
                        }
                    }
                    else
                    {
                        _designAuditRiseService.GetExp3FileFromZip(fullFileUploadPath, fullExpFileTempPath);
                        expFilePath = fullExpFileTempPath;
                    }

                    /*
                    *.exp to .dat file
                    *先執行一次exp to dat。轉出完整part.dat，勾選比對分頁後，在於比對時產出分頁篩選過後的part.dat
                    */
                    var resEXPtoData = _designAuditRiseService.ExpFileToDatFile(expFilePath).Result;
                    if (string.IsNullOrEmpty(resEXPtoData))
                    {
                        var datFiles = Directory.EnumerateFiles(expFilePath).Where(c => !c.EndsWith(".exp1", StringComparison.OrdinalIgnoreCase)).ToArray();
                        foreach (var f in datFiles)
                        {
                            System.IO.File.Copy(f, $"{fullDatFileTempPath}//{Path.GetFileName(f)}", true);
                            System.IO.File.Delete(f);
                        }
                    }
                    else
                    {
                        result.Message = resEXPtoData;
                        return result;
                    }

                    //file.exp1路徑                
                    var exp1Path = Utils.GetExp1FilePath(expFilePath);

                    //刪除上傳檔案，只留exp檔案資料夾
                    System.IO.File.Delete(fullFileUploadPath);

                    if (!remoteExp1 && !Path.Exists(exp1Path))
                    {
                        result.Message = $"查無exp1檔，無法取得分頁資訊.";
                        return result;
                    }

                    //file.exp1轉Entity
                    var schematicData = _designAuditRiseService.GetSchematicInfo(exp1Path).Result;
                    if (!string.IsNullOrEmpty(schematicData.errorMsg))
                    {
                        result.Message = schematicData.errorMsg;
                        return result;
                    }
                    result.Content.SchematicDatas = schematicData.schematics;


                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            finally
            {
                lockDic.Remove(userName);
            }

            return result;
        }

        /// <summary>
        /// OrCad上傳zip檔後呼叫，進行解壓縮取得、exp1檔exp轉.dat檔。供自動載入功能使用
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Result<string>> ToExp3FileFromOrcad([FromQuery] string? sourceFileName, string? destFileName)
        {
            var result = new Result<string>() { };
            var securitySourcePath = string.Empty;
            var securityDestinationPath = string.Empty;
            var errorMsg = string.Empty;
            var res = string.Empty;
            var dic = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(sourceFileName))
            {
                securitySourcePath = Utils.SecurityPathCombine(zipFileUploadPath, sourceFileName);

                if (!System.IO.File.Exists(securitySourcePath))
                {
                    result.Message = "查無來源檔(Source)，請確認是否上傳.";
                    return result;
                }

                dic.Add($"sourceId_{Guid.NewGuid()}", securitySourcePath);
            }

            if (!string.IsNullOrEmpty(destFileName))
            {
                securityDestinationPath = Utils.SecurityPathCombine(zipFileUploadPath, destFileName);

                if (!System.IO.File.Exists(securityDestinationPath))
                {
                    result.Message = "查無比對檔(Destination)，請確認是否上傳.";
                    return result;
                }

                dic.Add($"destId_{Guid.NewGuid()}", securityDestinationPath);
            }

            var tasks = new Task[dic.Count];
            for (int i = 0; i < tasks.Length; i++)
            {
                var dicItem = dic.ToArray()[i];
                tasks[i] = Task.Run(() =>
                {
                    var extractPath = Utils.SecurityPathCombine(expFileTempPath, "OrCad", dicItem.Key.Split("_")[1]);
                    var datFilePath = Utils.SecurityPathCombine(datFileTempPath, "OrCad", dicItem.Key.Split("_")[1]);

                    if (!Directory.Exists(extractPath))
                    {
                        Directory.CreateDirectory(extractPath);
                    }

                    if (!Directory.Exists(datFilePath))
                    {
                        Directory.CreateDirectory(datFilePath);
                    }

                    try
                    {
                        //解壓縮zip檔
                        _designAuditRiseService.GetExp3FileFromZip(dicItem.Value, extractPath);

                        //.exp檔 => .dat檔
                        var resEXPtoData = _designAuditRiseService.ExpFileToDatFile(extractPath).Result;
                        if (string.IsNullOrEmpty(resEXPtoData))
                        {
                            var datFiles = Directory.EnumerateFiles(extractPath).Where(c => !c.EndsWith(".exp1", StringComparison.OrdinalIgnoreCase)).ToArray();
                            foreach (var f in datFiles)
                            {//搬移.dat檔
                                System.IO.File.Copy(f, $"{datFilePath}//{Path.GetFileName(f)}", true);
                                System.IO.File.Delete(f);
                            }
                        }
                        else
                        {
                            throw new Exception(resEXPtoData);
                        }

                        lock (lockObj)
                        {
                            res += $"&{dicItem.Key.Split("_")[0]}={dicItem.Key.Split("_")[1]}";
                        }                            
                    }
                    catch (Exception ex)
                    {
                        lock (lockObj)
                        {
                            errorMsg += $"{Path.GetFileName(dicItem.Value)} processing Error, {ex.Message}. ";
                        }
                        
                        Directory.Delete(extractPath, true);
                        Directory.Delete(datFilePath, true);
                    }
                    finally
                    {
                        //刪除上傳檔案
                        System.IO.File.Delete(dicItem.Value);
                    }

                });
            }

            Task.WaitAll(tasks);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                result.Message = errorMsg;
                return result;
            }

            result.Content = $"{webSiteUrl}?{res.TrimStart('&')}";
            result.Success = true;

            return result;
        }

        /// <summary>
        /// 取得exp1並轉換成Schematic 轉換成Entity，在轉換成SchematicPageInfoModel
        /// </summary>
        /// <param name="sourcefileId"></param>
        /// <param name="destfileId"></param>
        /// <param name="subPath">子資料夾名稱，預設為UserName</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Result<Dictionary<string, List<SchematicInfoModel>>>> GetSchematic(string? sourcefileId, string? destfileId, string? subPath = "")
        {
            var errorMsg = string.Empty;
            var result = new Result<Dictionary<string, List<SchematicInfoModel>>>();
            var dic = new Dictionary<string, string>();
            subPath = string.IsNullOrEmpty(subPath) ? User.Identity.Name.Split('\\')[1] : subPath;

            if (!string.IsNullOrEmpty(sourcefileId))
            {
                dic.Add($"Source", sourcefileId);
            }
            if (!string.IsNullOrEmpty(destfileId))
            {
                dic.Add($"Destination", destfileId);
            }

            var tasks = new Task[dic.Count];
            for (int i = 0; i < tasks.Length; i++)
            {
                var dicItem = dic.ToArray()[i];
                tasks[i] = Task.Run(async () =>
                    {
                        //file.exp1轉Entity
                        var exp1Folder = Utils.SecurityPathCombine(expFileTempPath, subPath, dicItem.Value);
                        var exp1FullPath = Directory.EnumerateFiles(exp1Folder).Where(p => p.EndsWith(".exp1", StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? "";

                        var schematicData = await _designAuditRiseService.GetSchematicInfo(exp1FullPath);
                        if (!string.IsNullOrEmpty(schematicData.errorMsg))
                        {
                            lock (lockObj)
                            {
                                errorMsg += $"{dicItem.Key} file transfer to Entity fail. {schematicData.errorMsg}。";
                            }
                        }
                        else
                        {
                            lock (lockObj)
                            {
                                result.Content.Add(dicItem.Key, schematicData.schematics);
                            }
                        }
                    }
                );
            }
            Task.WaitAll(tasks);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                result.Message = errorMsg;
                return result;
            }
            
            result.Success = true;
            return result;
        }

        /// <summary>
        /// dsn比對
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Result<ResComparePageDto>> DsnCompare([FromBody] Dtos.Request.ReqComparePageDto data)
        {
            var result = new Result<ResComparePageDto>();

            try
            {
                var userName = User.Identity.Name.Split('\\')[1];
                var rootSourceExpFileTempPath = Utils.SecurityPathCombine(expFileTempPath, string.IsNullOrEmpty(data.SourceId) ? userName : "OrCad", string.IsNullOrEmpty(data.SourceId) ? SchematicType.Source : data.SourceId);
                var rootDestinationExpFileTempPath = Utils.SecurityPathCombine(expFileTempPath, string.IsNullOrEmpty(data.DestId) ? userName : "OrCad", string.IsNullOrEmpty(data.DestId) ? SchematicType.Destination : data.DestId);
                var rootSourceDatFileTempPath = Utils.SecurityPathCombine(datFileTempPath, string.IsNullOrEmpty(data.SourceId) ? userName : "OrCad", string.IsNullOrEmpty(data.SourceId) ? SchematicType.Source : data.SourceId, "part.dat");
                var rootDestinationDatFileTempPath = Utils.SecurityPathCombine(datFileTempPath, string.IsNullOrEmpty(data.DestId) ? userName : "OrCad", string.IsNullOrEmpty(data.DestId) ? SchematicType.Destination : data.DestId, "part.dat");

                //Get [Source] Compare Parameter
                var sourceExp1Path = Utils.GetExp1FilePath(rootSourceExpFileTempPath);
                var sourceExp1Entity = await _designAuditRiseService.Exp1FileToEntity(sourceExp1Path);
                var sourceSelectedPage = data.SelectedSourceSchematicPage.Select(c => c.item).ToArray();
                var sourceCloneExp1Entity = await _designAuditRiseService.GetFilterPage(sourceExp1Entity, sourceSelectedPage);
                //.exp to .dat file for Schematic Viewer。產生篩選分頁後的dat檔案跟Schematic Viewer做檔案交換。
                var sourceEXPtoData = await _designAuditRiseService.CreateProtobuffZip(rootSourceDatFileTempPath, sourceSelectedPage);
                if (!string.IsNullOrEmpty(sourceEXPtoData.errMsg))
                {
                    result.Message = sourceEXPtoData.errMsg;
                    return result;
                }

                //Get [Destination] Compare Parameter
                var destinationExp1Path = Utils.GetExp1FilePath(rootDestinationExpFileTempPath);
                var destinationExp1Entity = await _designAuditRiseService.Exp1FileToEntity(destinationExp1Path);
                var destinationSelectedPage = data.SelectedDestinationSchematicPage.Select(c => c.item).ToArray();
                var destinationCloneExp1Entity = await _designAuditRiseService.GetFilterPage(destinationExp1Entity, destinationSelectedPage);
                //.exp to .dat file for Schematic Viewer。產生篩選分頁後的dat檔案跟Schematic Viewer做檔案交換。
                var destinationEXPtoData = await _designAuditRiseService.CreateProtobuffZip(rootDestinationDatFileTempPath, destinationSelectedPage);
                if (!string.IsNullOrEmpty(destinationEXPtoData.errMsg))
                {
                    result.Message = destinationEXPtoData.errMsg;
                    return result;
                }

                //Dsn Compare
                var oneWay = data.CompareMode == "0" ? true : false;
                var res = await _designAuditRiseService.DsnCompare(sourceExp1Path, destinationExp1Path, sourceCloneExp1Entity, destinationCloneExp1Entity, oneWay, DsnAllias[0], DsnAllias[1]);
                res.PartDiffs.ForEach(c => { c.ObjID = string.IsNullOrEmpty(c.ObjID) ? Guid.NewGuid().ToString() : c.ObjID; });
                res.NetDiffs.ForEach(c => { c.ObjID = string.IsNullOrEmpty(c.ObjID) ? Guid.NewGuid().ToString() : c.ObjID; });

                var rnd = new Random();
                var reportID = string.Format("{0:000000000}", rnd.Next(1, 999999999));
                var fileName = string.Empty;
                result.Content.designSN1 = $"S_{reportID}";
                result.Content.designSN2 = $"D_{reportID}";
                result.Content.designDiff = res;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 比對結果下載
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        //[ExceptionAttribute]
        public async Task<IActionResult> ExportCompareResult([FromBody] DesignDiff data)
        {
            var dt = new DataTable();
            var tempType = string.Empty;
            var header = new string[] { };

            try
            {
                if (data.PartDiffs != null && data.PartDiffs.Count > 0)
                {
                    header = new string[] {
                        "ObjID",
                        "Schematic",
                        "Page",
                        "Reference",
                        "Part_Reference",
                        "Error_Category",
                        "Error_Description",
                        "Design1",
                        "Design2",
                        "Comment",
                        "Design1ObjID",
                        "Design2ObjID",
                        "Design1Page",
                        "Design2Page"
                    };
                    dt = ConvertUtils.ListToDataTable(data.PartDiffs);
                    tempType = TempPathType.PartCompareResult;
                }
                else if (data.NetDiffs != null && data.NetDiffs.Count > 0)
                {
                    header = new string[] {
                        "ObjID",
                        "Schematic",
                        "Page",
                        "Pages_NetPinNames",
                        "Reference",
                        "Part_Reference",
                        "Net_Pin_Name",
                        "Error_Category",
                        "Error_Description",
                        "Design1",
                        "Design2",
                        "Comment",
                        "Design1Schematic",
                        "Design2Schematic",
                        "Design1Page",
                        "Design2Page",
                        "Design1PinName",
                        "Design1PinNumber",
                        "Design2PinName",
                        "Design2PinNumber",
                    };
                    dt = ConvertUtils.ListToDataTable(data.NetDiffs);
                    tempType = TempPathType.NetCompareResult;
                }

                var fs = await _excelService.ExportExcel(tempType, dt, header);
                return fs;
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [ServiceFilter(typeof(AllowedIpAttribute))]
        //[DesignAuditRise.Web.Filters.AllowedIp(_allowIpList)]
        [Route("{darType}/{folderName}")]        
        public async Task<IActionResult> GetDrawingData(string darType, string folderName)
        {           
            try
            {
                string filePath = string.Empty;

                if (darType.ToLower() == "orcad")
                {//darType：orcad, folderName：fileId
                    filePath = Utils.SecurityPathCombine(datFileTempPath, darType, folderName, _designAuditRiseService.ProtoBuffDataFileName);
                }
                else
                {//darType：source/destination, folderName：user_name
                    filePath = Utils.SecurityPathCombine(datFileTempPath, folderName, darType, _designAuditRiseService.ProtoBuffDataFileName);
                }

                if (System.IO.File.Exists(filePath))
                {
                    var stream = new FileStream(filePath, FileMode.Open);
                    return File(stream, "application/octet-stream");
                }
                else
                {
                    return BadRequest("File not exist.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
