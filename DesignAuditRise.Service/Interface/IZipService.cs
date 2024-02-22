using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignAuditRise.Service.Interface
{
    public interface IZipService
    {
        public void ExtractZip(string zipFilePath, string extractPath, string? filterString = null);
        public void Zip(string savePath, string[] files);
    }
}
