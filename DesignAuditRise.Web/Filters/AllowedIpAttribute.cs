using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DesignAuditRise.Web.Filters
{
    public class AllowedIpAttribute: ActionFilterAttribute
    {
        private string[] ipList = new string[] { };

        public AllowedIpAttribute(string allowedIps)
        {
            ipList = allowedIps.Split(',', ';');
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var clientIp = context.HttpContext.Connection.RemoteIpAddress.ToString();            

            if (!ipList.Contains(clientIp)) 
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 400,
                    Content = "ip not allowed!!",
                    ContentType = "text/plain; charset=utf-8"
                };
            }

            base.OnActionExecuting(context);
        }
    }
}
