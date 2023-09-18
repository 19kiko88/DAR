namespace DesignAuditRise.Web.Dtos.Request
{
    public class ReqToExp3File
    {
        public FileType FileType { get; set; }
        public string UploadFileName { get; set; }        
    }

    public enum FileType
    {
        Source,
        Destination
    }
}
