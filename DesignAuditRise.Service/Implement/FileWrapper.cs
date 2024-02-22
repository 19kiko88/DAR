using DesignAuditRise.Service.Interface;
using System.IO.Abstractions;

namespace DesignAuditRise.Service.Implement
{
    public class FileWrapper : IFileWrapper
    {
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// 單元測試用，傳入mockFileSystem物件。於單元測試時不真的執行System.IO的函式(ex: File.Copy...etc)
        /// </summary>
        /// <param name="fileSystem"></param>
        public FileWrapper(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// 建立FileSystem物件，override 原本System.IO的函式(ex: File.Copy...etc)
        /// </summary>
        public FileWrapper()
        {
            _fileSystem = new FileSystem();
        }

        #region File實作
        public void FileCopy(string sourceFileName, string destFileName, bool overwrite)
        {
            _fileSystem.File.Copy(sourceFileName, destFileName, overwrite);
        }

        public bool FileExists(string? path)
        {
            return _fileSystem.File.Exists(path);
        }
        #endregion

        #region Directory 實作
        public bool DirectoryExists(string path)
        {
            return _fileSystem.Directory.Exists(path);
        }

        public void DirectoryCreate(string path)
        {
            _fileSystem.Directory.CreateDirectory(path);
        }

        public string[] DirectoryFilesGet(string path, string searchPattern, SearchOption searchOption)
        {
            return _fileSystem.Directory.GetFiles(path, searchPattern, searchOption);
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            _fileSystem.Directory.Delete(path);
        }

        public IEnumerable<string> DirectoryEnumerateFiles(string path)
        {
            return _fileSystem.Directory.EnumerateFiles(path);
        }
        #endregion
    }
}
