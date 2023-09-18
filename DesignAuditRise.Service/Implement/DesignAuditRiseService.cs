using AutoMapper;
using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;
using DesignAuditRise.Service.Interface;
using ICSharpCode.SharpZipLib.Zip;

using UniversalLibrary.Models;

namespace DesignAuditRise.Service.Implement
{
    public class DesignAuditRiseService : IDesignAuditRiseService
    {
        private readonly OuterService.Interface.IConvertPediaService _outerConvertPediaService;
        private readonly OuterService.Interface.IDesignAuditRiseService _designAuditRiseService;
        private readonly IMapper _mapper;

        public DesignAuditRiseService(
            OuterService.Interface.IConvertPediaService convertPediaService,
            OuterService.Interface.IDesignAuditRiseService designAuditRiseService
        )
        {
            _outerConvertPediaService = convertPediaService;
            _designAuditRiseService = designAuditRiseService;
        }

        public async Task<Result> GetExp3FileFromDSN(string dsnFilePath)
        {
            return _outerConvertPediaService.ToExp3File(dsnFilePath).Result;
        }

        public async Task GetExp3FileFromZIP(string zipFilePath, string expFileTempPath)
        {
            FastZip fastZip = new FastZip();
            string fileFilter = null;

            //建立暫存解壓縮資料的資料夾
            var extractPath = zipFilePath.Replace(".zip", "");
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            //解壓縮
            fastZip.ExtractZip(zipFilePath, extractPath, fileFilter);

            //篩選附檔名為.exp, .exp1的壓縮資料
            var filterFiles = 
                Directory.EnumerateFiles(extractPath, "*.*", SearchOption.AllDirectories)
                .Where(c => c.EndsWith(".exp") || c.EndsWith(".exp1"))
                .ToList();

            //搬移解壓縮檔案到exp file資料夾
            foreach ( var f in filterFiles)
            {
                File.Copy(f, Path.Combine(expFileTempPath, Path.GetFileName(f)), true);
            }

            //刪除暫存解壓縮資料的資料夾            
            Directory.Delete(extractPath, true);

        }

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

        public async Task<DesignDiff> DsnCompare(Exp1 sourceExp1, Exp1 destinationExp1, Exp1 sourceFilterExp1, Exp1 destinationFilterExp1, string sourceDsnAlias, string destinationDsnAlias, bool oneWay)
        {            
            var res = _designAuditRiseService.DsnCompare(sourceExp1, destinationExp1, sourceFilterExp1, destinationFilterExp1, sourceDsnAlias, destinationDsnAlias, oneWay);
            if (res.Success)
            {
                return res.Content;
            }
            else
            {
                return null;
            }
        }
    }
}
