namespace DesignAuditRise.Web.Dtos.Response
{
    public class SchematicData
    {
        public string item { get; set; }
        public List<PageData> Children { get; set; }
    }

    public class PageData
    {
        public string item { get; set; } = "";
        public int level { get; set; } = 0;
        public bool expandable { get; set; } = false;
    }
}
