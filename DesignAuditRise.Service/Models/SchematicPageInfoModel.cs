using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignAuditRise.Service.Models
{
    public class SchematicPageInfoModel
    {
        public string item { get; set; } = "";
        public int level { get; set; } = 0;
        public bool expandable { get; set; } = false;
    }
}
