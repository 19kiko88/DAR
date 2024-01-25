using CAEService;
using DesignAuditRise.Service.Models;
using DesignAuditRise.Service.Utility;
using Microsoft.EntityFrameworkCore;

namespace DesignAuditRise.Service.Implement
{
    public  class CommonService: DesignAuditRise.Service.Interface.ICommonService
    {
        private readonly int cacheExpireTime = 1440;//快取保留時間
        private readonly OuterService.Interface.ICommonOuterService _commonService;

        public CommonService(OuterService.Interface.ICommonOuterService commonService) 
        {
            _commonService = commonService;
        }

        public async Task<Employee> GetEmpInfo(string name)
        {
            return _commonService.GetEmployeeInfo(name); ;
        }
    }
}
