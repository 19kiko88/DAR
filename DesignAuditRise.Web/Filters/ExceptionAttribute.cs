using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UniversalLibrary.Models;

namespace DesignAuditRise.Web.Filters
{
    public class ExceptionAttribute: ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var result = new Result
            {
                Success = false,
                Message = context.Exception.Message
            };
            context.Result = new JsonResult(result);
        }
    }
}
