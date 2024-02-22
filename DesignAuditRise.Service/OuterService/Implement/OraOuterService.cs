using CAEService;
using DesignAuditRise.Service.OuterService.Interface;
using DesignAuditRise.Web;
using Microsoft.Extensions.Options;

namespace DesignAuditRise.Service.OuterService.Implement
{
    public class OraOuterService: IOraOuterService
    {
        private readonly AppSettingsInfoModel.ConnectionStrings _conn;
        private readonly OraServiceClient _serviceClient;

        public OraOuterService(IOptions<AppSettingsInfoModel.ConnectionStrings> conn) 
        {
            if (conn != null)
            {
                _conn = conn.Value;
                _serviceClient = new OraServiceClient(OraServiceClient.EndpointConfiguration.BasicHttpBinding_IOraService, _conn.CAEServiceConnection);
            }            
        }

        public Employee GetEmployeeInfo(string username)
        {
            var emps = _serviceClient.GetEIPEmployeeDataAsync("Name", username, false, true).Result.GetEIPEmployeeDataResult;
            if (emps.IsMatch && emps.Employees.Any(a => a.Quit == "N"))
            {
                return emps.Employees.Where(a => a.Quit == "N").First();
            }
            return null;
        }
    }
}
