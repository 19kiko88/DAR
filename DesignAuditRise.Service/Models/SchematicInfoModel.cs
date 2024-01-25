using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignAuditRise.Service.Models
{
    public class SchematicInfoModel
    {
        public string item { get; set; }
        public List<SchematicPageInfoModel> Children { get; set; }
    }
}
