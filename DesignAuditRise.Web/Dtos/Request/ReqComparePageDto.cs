using DesignAuditRise.Service.Models;

namespace DesignAuditRise.Web.Dtos.Request
{
    public class ReqComparePageDto
    {
        public string CompareMode { get; set; }
        public List<SchematicPageInfoModel> SelectedSourceSchematicPage { get; set; }
        public List<SchematicPageInfoModel> SelectedDestinationSchematicPage { get; set; }

    }
}
