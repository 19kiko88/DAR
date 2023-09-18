using Converpedia.Models.Dsn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalLibrary.Models;

namespace DesignAuditRise.Service.OuterService.Interface
{
    public interface IConvertPediaService
    {
        /// <summary>
        /// DSN檔案透過遠端Server轉檔成file.exp1, cache.exp, font.exp, part.exp。
        /// </summary>
        /// <param name="dsnFilePath">dsn檔案路徑</param>
        /// <returns>遠端Server所存轉檔路徑</returns>
        public Task<Result> ToExp3File(string dsnFilePath);

        /// <summary>
        /// file.exp1轉成Entity class
        /// </summary>
        /// <param name="exp1Path">file.exp1路徑</param>
        /// <returns></returns>
        public Task<Result<Exp1>> Exp1FileToEntity(string exp1Path);

        /// <summary>
        /// part.exp轉成Entity class
        /// </summary>
        /// <param name="expPartPath">part.exp路徑</param>
        /// <returns></returns>
        public Task<Result<DesignInfo>> PartFileToEntities(string expPartPath);

        /// <summary>
        /// font.exp轉成Entity class
        /// </summary>
        /// <param name="expFontPath">font.exp路徑</param>
        /// <returns></returns>
        public Task<Result<List<DesignFont>>> FontFileToEntities(string expFontPath);

        /// <summary>
        /// cache.exp轉成Entity class
        /// </summary>
        /// <param name="expCachePath">cache.exp路徑</param>
        /// <returns></returns>
        public Task<Result<List<DesignCache>>> CacheFileToEntities(string expCachePath);
    }
}
