using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace DesignAuditRise.Web.Filters
{
    public class AllowedIpAttribute: ActionFilterAttribute
    {
        private readonly ILogger<AllowedIpAttribute> _logger;
        private string[] _ipList;

        public AllowedIpAttribute(
            ILoggerFactory logger,
            IOptions<AppSettingsInfoModel.OtherSettings> settings
            )
        {
            /*
             *Ref：ASP.NET Core - 在ActionFilter中使用依賴注入
             *https://www.twblogs.net/a/5e7b0011bd9eee2116864f2e
             */
            _logger = logger.CreateLogger<AllowedIpAttribute>();
            _ipList = settings.Value?.IpFilters;          
        }

        //public AllowedIpAttribute(string allowedIps)
        //{
        //    ipList = allowedIps.Split(',', ';');
        //}

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var clientIp = context.HttpContext.Connection.RemoteIpAddress.ToString();

            if (!_ipList.Contains(clientIp)) 
            {
                _logger.LogError($"非法存取IP： [{clientIp}] ip not allowed!!");

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
