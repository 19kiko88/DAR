using DesignAuditRise.Service.OuterService.Interface;
using DsnLibrary;

namespace DesignAuditRise.Service.OuterService.Implement
{
    public class ProcessDsnOuterService: IProcessDsnOuterService
    {
        private ProcessDSN _processDSN;


        public (bool res, string errMsg) EXPtoDatFile(string expFilePath, string folderName = "")
        {
            _processDSN = new ProcessDSN(expFilePath);
            
            var errMsg = string.Empty;
            var res = _processDSN.EXPtoData(folderName, out errMsg);

            return (res: res, errMsg: errMsg);
        }
    }
}
