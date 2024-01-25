using DesignAuditRise.Web.Models;

namespace DesignAuditRise.Web.Models
{
    public class Schematic
    {
        public string item { get; set; }
        public List<SchematicPage> Children { get; set; }
    }
}
