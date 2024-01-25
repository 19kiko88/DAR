using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversalLibrary.Models;

namespace DesignAuditRise.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CommonController: ControllerBase
    {
        private readonly DesignAuditRise.Service.Interface.ICommonService _commonService;

        public CommonController(DesignAuditRise.Service.Interface.ICommonService commonService)
        {
            _commonService = commonService;
        }

        /// <summary>
        /// 取得UserName
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public async Task<Result<string>> GetUserInfo()
        {
            var result = new Result<string>() { };
            try
            {
                var empInfo = await _commonService.GetEmpInfo(User.Identity.Name.Split('\\')[1]);
                result.Content = $"{empInfo.Name}({empInfo.NameCT})";
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
