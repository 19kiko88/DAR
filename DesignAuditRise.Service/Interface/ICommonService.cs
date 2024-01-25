using CAEService;

namespace DesignAuditRise.Service.Interface
{
    public interface ICommonService
    {
        public Task<Employee> GetEmpInfo(string name);
    }
}
