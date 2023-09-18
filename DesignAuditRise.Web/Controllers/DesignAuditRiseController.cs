using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalLibrary.Models;
using DesignAuditRise.Service.Interface;
using Microsoft.Extensions.Options;
using DesignAuditRise.Web.Dtos.Response;
using DesignAuditRise.Web.Dtos.Request;
using DesignAuditRise.Web.Models;
using System.Data;
using DesignAuditPedia.Models.Dsn.DesignCompare;

namespace DesignAuditRise.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DesignAuditRiseController : ControllerBase
    {
        private readonly IDesignAuditRiseService _designAuditRiseService;
        private readonly IWebHostEnvironment _hostEnv;
        private string dsnFileUploadPath = string.Empty;
        private string zipFileUploadPath = string.Empty;
        private string expFileTempPath = string.Empty;
        private string[] DsnAllias = new string[2];

        public DesignAuditRiseController(
            IDesignAuditRiseService designAuditRiseService,
            IWebHostEnvironment hostEnv,
            IOptions<AppSettings.PathSettings> pathSettings,
            IOptions<AppSettings.OtherSettings> otherSettings
        )
        { 
            _designAuditRiseService = designAuditRiseService;
            _hostEnv = hostEnv;
            dsnFileUploadPath = Path.Combine(hostEnv.ContentRootPath, pathSettings.Value?.DsnFileUploadPath);
            zipFileUploadPath = Path.Combine(hostEnv.ContentRootPath, pathSettings.Value?.ZipFileUploadPath);
            expFileTempPath = Path.Combine(hostEnv.ContentRootPath, pathSettings.Value?.ExpFileTempPath);
            DsnAllias = otherSettings.Value?.DsnAlias;
        }

        /// <summary>
        /// 檔案上傳
        /// Ref：https://arminzia.com/blog/file-upload-with-aspnet-core-and-angular/
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Result<ResUploadFileInfo>> Upload()
        {
            var result = new Result<ResUploadFileInfo>();

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
                
                var uploadFileInfo = new ResUploadFileInfo();
                var file = Request.Form.Files[0];
                var extension = Path.GetExtension(file.FileName);
                uploadFileInfo.OldFileName = file.FileName;                
                uploadFileInfo.NewFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}{extension}";

                var fullPath = Path.Combine(extension == ".dsn" ? dsnFileUploadPath : zipFileUploadPath, uploadFileInfo.NewFileName);
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

        [HttpPost]
        public async Task<Result<List<SchematicData>>> ToExp3File([FromBody] ReqToExp3File data)
        {
            var result = new Result<List<SchematicData>>() { };
            try
            {
                var remoteExp1 = false;
                var extension = Path.GetExtension(data.UploadFileName);
                var expFilePath = string.Empty;
                var fullFileUploadPath = Path.Combine(extension == ".dsn" ? dsnFileUploadPath : zipFileUploadPath, data.UploadFileName);
                var fullExpFileTempPath = Path.Combine(expFileTempPath, User.Identity.Name.Split('\\')[1], data.FileType == FileType.Source ? "Source" : "Destination");

                Directory.Delete(fullExpFileTempPath, true);
                Directory.CreateDirectory(fullExpFileTempPath);

                if (extension == ".dsn")
                {
                    //取得server的exp檔案資料夾
                    var res = await _designAuditRiseService.GetExp3FileFromDSN(fullFileUploadPath);

                    
                    if (res.Success)
                    {
                        expFilePath = res.Message;
                        try
                        {
                            //搬移exp檔案到本機資料夾
                            foreach (var f in Directory.GetFiles(expFilePath))
                            {
                                System.IO.File.Copy(Path.Combine(expFilePath, f), Path.Combine(fullExpFileTempPath, f), true);
                            }

                            expFilePath = fullExpFileTempPath;
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
                    await _designAuditRiseService.GetExp3FileFromZIP(fullFileUploadPath, fullExpFileTempPath);
                    expFilePath = fullExpFileTempPath;
                }

                //file.exp1路徑
                var exp1Path = Path.Combine(expFilePath, "file.exp1");

                //刪除上傳檔案，只留exp檔案資料夾
                System.IO.File.Delete(fullFileUploadPath);

                if (!remoteExp1 && !Path.Exists(exp1Path))
                {
                    result.Message = $"查無exp1檔，無法取得分頁資訊.";
                    return result;
                }

                //file.exp1轉Entity
                var exp1Entity = await _designAuditRiseService.Exp1FileToEntity(exp1Path);

                if (exp1Entity.Schematics.Count <= 0)
                {
                    result.Message = "exp1檔案查無Schematics相關訊息.";
                    return result;
                }


                var listSchematic = new List<SchematicData>();
                var schematic = new SchematicData()
                {
                    Children = new List<PageData>()
                };

                foreach (var itemSchematic in exp1Entity.Schematics)
                {
                    schematic.item = itemSchematic.Name;
                    
                    foreach (var page in itemSchematic.Pages)
                    {
                        var child = new PageData();
                        child.item = page.Name;
                        child.level = 2;
                        schematic.Children.Add(child);
                    }

                    listSchematic.Add(schematic);
                }

#if DEBUG
                listSchematic.Add(new SchematicData()
                {
                    item = "SCHEMAGIC_homer_test",
                    Children = new List<PageData>()
                    {
                        new PageData()
                        {
                            item = "PAGE1_homer_test",
                            level = 2
                        },
                        new PageData()
                        {
                            item = "PAGE2_homer_test",
                            level = 2
                        }
                    }
                });
#endif

                result.Content = listSchematic;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        [HttpPost]
        public async Task<Result<DesignDiff>> DsnCompare([FromBody] ReqComparePage data)
        {
            var result = new Result<DesignDiff>();

            try
            {
                var rootExpFileTempPath = $"{expFileTempPath}{User.Identity.Name.Split('\\')[1]}" ;
                var exp1FileName = "file.exp1";

                //Get [Source] Compare Parameter
                var sourceExp1Path = Path.Combine(rootExpFileTempPath, SchematicType.Source, exp1FileName);
                var sourceExp1Entity = await _designAuditRiseService.Exp1FileToEntity(sourceExp1Path);
                var sourceSelectedPage = data.SelectedSourceSchematicPage.Select(c => c.item).ToArray();
                var sourceCloneExp1Entity = await _designAuditRiseService.GetFilterPage(sourceExp1Entity, sourceSelectedPage);

                //Get [Destination] Compare Parameter
                var destinationExp1Path = Path.Combine(rootExpFileTempPath, SchematicType.Destination, exp1FileName);
                var destinationExp1Entity = await _designAuditRiseService.Exp1FileToEntity(destinationExp1Path);
                var destinationSelectedPage = data.SelectedDestinationSchematicPage.Select(c => c.item).ToArray();
                var destinationCloneExp1Entity = await _designAuditRiseService.GetFilterPage(destinationExp1Entity, destinationSelectedPage);

                //Dsn Compare
                var oneWay = data.CompareMode == "0" ? true : false;
                var res = await _designAuditRiseService.DsnCompare(sourceExp1Entity, sourceCloneExp1Entity, destinationExp1Entity, destinationCloneExp1Entity, DsnAllias[0], DsnAllias[1], oneWay);

#if DEBUG
                res.PartDiffs.ForEach(c => { c.ObjID = string.IsNullOrEmpty(c.ObjID) ? Guid.NewGuid().ToString() : c.ObjID; });
                res.NetDiffs.ForEach(c => {c.ObjID = string.IsNullOrEmpty(c.ObjID) ? Guid.NewGuid().ToString() : c.ObjID; });
#endif

                result.Content = res;
                result.Success = true;               
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
