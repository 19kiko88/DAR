
using UniversalLibrary.Models;
using DesignAuditPedia.Dsn;
using DesignAuditRise.Service.OuterService.Interface;
using Converpedia.Models.Dsn;
using DesignAuditPedia.Models.Dsn.DesignCompare;

namespace DesignAuditRise.Service.OuterService.Implement
{
    public class DesignAuditRiseService : IDesignAuditRiseService
    {
        private readonly IConvertPediaService _convertPediaService;
        DesignCompare _designCompareService;


        public DesignAuditRiseService(IConvertPediaService convertPediaService)
        {
            _convertPediaService = convertPediaService;
            _designCompareService = new DesignCompare();
        }

        public Result<DesignDiff> DsnCompare(Exp1 sourceExp1, Exp1 destinationExp1, Exp1 sourceFilterExp1, Exp1 destinationFilterExp1, string sourceDsnAlias, string destinationDsnAlias, bool oneWay)
        {
            return _designCompareService.SkrDsnCompare(sourceExp1, destinationExp1, sourceDsnAlias, destinationDsnAlias, sourceFilterExp1, destinationFilterExp1, oneWay);
        }
    }
}
