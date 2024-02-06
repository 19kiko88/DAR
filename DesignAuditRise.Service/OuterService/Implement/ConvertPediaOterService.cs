using Converpedia.Schematic;
using Converpedia.Models.Dsn;
using DesignAuditRise.Web;
using Microsoft.Extensions.Options;
using UniversalLibrary.Models;
using DesignAuditRise.Service.OuterService.Interface;

namespace DesignAuditRise.Service.OuterService.Implement
{
    public class ConvertPediaOterService : IConvertPediaOuterService
    {
        private readonly Exp1Service _exp1Service;
        private readonly DsnService _dsnService;
        private readonly ViewerService _viewerService;


        public ConvertPediaOterService(IOptions<AppSettingsInfoModel.DsnServiceParameters> dsnServiceParameters)
        {
            _viewerService = new ViewerService();
            _exp1Service = new Exp1Service();
            _dsnService = new DsnService(dsnServiceParameters?.Value.ServicePwd, dsnServiceParameters?.Value.SendTimeout);
        }

        /// <summary>
        /// DSN檔案透過遠端Server轉檔成file.exp1, cache.exp, font.exp, part.exp。
        /// </summary>
        /// <param name="dsnFilePath">dsn檔案路徑</param>
        /// <returns>遠端Server所存轉檔路徑</returns>
        public async Task<Result> ToExp3File(string dsnFilePath)
        {
            return await _dsnService.ToExp3NewFile(dsnFilePath);
        }

        /// <summary>
        /// file.exp1轉成Entity class
        /// </summary>
        /// <param name="exp1Path">file.exp1路徑</param>
        /// <returns></returns>
        public async Task<Result<Exp1>> Exp1FileToEntity(string exp1Path)
        {
            return await _exp1Service.Exp1NewFileToEntity(exp1Path);
        }

        /// <summary>
        /// part.exp轉成Entity class
        /// </summary>
        /// <param name="expPartPath">part.exp路徑</param>
        /// <returns></returns>
        public async Task<Result<DesignInfo>> PartFileToEntities(string expPartPath)
        {
            return await _viewerService.PartFileToEntities(expPartPath);
        }

        /// <summary>
        /// font.exp轉成Entity class
        /// </summary>
        /// <param name="expFontPath">font.exp路徑</param>
        /// <returns></returns>
        public async Task<Result<List<DesignFont>>> FontFileToEntities(string expFontPath)
        {
            return await _viewerService.FontFileToEntities(expFontPath);
        }

        /// <summary>
        /// cache.exp轉成Entity class
        /// </summary>
        /// <param name="expCachePath">cache.exp路徑</param>
        /// <returns></returns>
        public async Task<Result<List<DesignCache>>> CacheFileToEntities(string expCachePath)
        {
            return await _viewerService.CacheFileToEntities(expCachePath);
        }
    }
}
