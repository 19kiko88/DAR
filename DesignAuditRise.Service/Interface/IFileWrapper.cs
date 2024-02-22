using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignAuditRise.Service.Interface
{
    public interface IFileWrapper
    {
        public void FileCopy(string sourceFileName, string destFileName, bool overwrite);
        public bool FileExists(string? path);
        public bool DirectoryExists(string path);
        public string[] DirectoryFilesGet(string path, string searchPattern, SearchOption searchOption);
        public void DirectoryCreate(string path);
        public void DirectoryDelete(string path, bool recursive);
        public IEnumerable<string> DirectoryEnumerateFiles(string path);
    }
}
