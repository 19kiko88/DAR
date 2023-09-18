using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;
using UniversalLibrary.Models;

namespace DesignAuditRise.Service.OuterService.Interface
{
    public interface IDesignAuditRiseService
    {
        public Result<DesignDiff> DsnCompare(Exp1 sourceExp1, Exp1 destinationExp1, Exp1 sourceFilterExp1, Exp1 destinationFilterExp1, string sourceDsnAlias, string destinationDsnAlias, bool oneWay);
    }
}
