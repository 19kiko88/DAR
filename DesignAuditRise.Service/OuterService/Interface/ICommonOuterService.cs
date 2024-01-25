using CAEService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignAuditRise.Service.OuterService.Interface
{
    public interface ICommonOuterService
    {
        public Employee GetEmployeeInfo(string username);
    }
}
