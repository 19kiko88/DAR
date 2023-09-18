using DesignAuditRise.Web.Dtos.Response;

namespace DesignAuditRise.Web.Dtos.Request
{
    public class ReqComparePage
    {
        public string CompareMode { get; set; }
        public List<PageData> SelectedSourceSchematicPage { get; set; }
        public List<PageData> SelectedDestinationSchematicPage { get; set; }

    }
}
