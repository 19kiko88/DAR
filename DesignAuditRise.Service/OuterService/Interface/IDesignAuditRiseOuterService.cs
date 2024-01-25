using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;
using UniversalLibrary.Models;

namespace DesignAuditRise.Service.OuterService.Interface
{
    public interface IDesignAuditRiseOuterService
    {
        public Result<DesignDiff> SkrDsnCompare(string exp1Path1, string exp1Path2, string identity1 = "Design1", string identity2 = "Design2", Exp1 sch1Pages = null, Exp1 sch2Pages = null, bool oneWay = true);

    }
}
