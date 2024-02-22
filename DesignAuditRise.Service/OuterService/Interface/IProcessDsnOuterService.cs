namespace DesignAuditRise.Service.OuterService.Interface
{
    public interface IProcessDsnOuterService
    {
        public (bool res, string errMsg) EXPtoDatFile(string expFilePath, string folderName = "");
    }
}
