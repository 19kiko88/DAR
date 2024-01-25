
using UniversalLibrary.Models;
using DesignAuditPedia.Dsn;
using DesignAuditRise.Service.OuterService.Interface;
using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;

namespace DesignAuditRise.Service.OuterService.Implement
{
    public class DesignAuditRiseOterService : IDesignAuditRiseOuterService
    {
        DesignCompare _designCompareService;


        public DesignAuditRiseOterService()
        {
            _designCompareService = new DesignCompare();
        }

        /// <summary>
        /// 線路圖比較
        /// </summary>
        /// <param name="exp1Path1">source exp1 file path</param>
        /// <param name="exp1Path2">destination exp1 file path</param>
        /// <param name="identity1"></param>
        /// <param name="identity2"></param>
        /// <param name="sch1Pages">source selected page</param>
        /// <param name="sch2Pages">destination selected page</param>
        /// <param name="oneWay"></param>
        /// <returns></returns>
        public Result<DesignDiff> SkrDsnCompare(string exp1Path1, string exp1Path2, string identity1 = "Design1", string identity2 = "Design2", Exp1 sch1Pages = null, Exp1 sch2Pages = null, bool oneWay = true)
        {    
            return _designCompareService.SkrDsnCompare(exp1Path1, exp1Path2, identity1, identity2, sch1Pages, sch2Pages, oneWay);
        }
    }
}
