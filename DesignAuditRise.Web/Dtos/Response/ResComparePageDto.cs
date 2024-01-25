using DesignAuditPedia.Models.Dsn.DesignCompare;

namespace DesignAuditRise.Web.Dtos.Response
{
    public class ResComparePageDto
    {
        public string designSN1 { get; set; }
        public string designSN2 { get; set; }
        public DesignDiff designDiff { get; set; }
    }
}
