using DesignAuditRise.Service.Interface;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace DesignAuditRise.Service.Implement
{
    public class ZipService: IZipService
    {
        private readonly FastZip _zip;

        public ZipService() 
        { 
            _zip = new FastZip();
        }

        public void ExtractZip(string zipFilePath, string extractPath, string? filterString = null)
        {
            _zip.ExtractZip(zipFilePath, extractPath, filterString);
        }

        public void Zip(string savePath, string[] zipFiles)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            using (var zipStream = new ZipOutputStream(File.Create(savePath)))
            {
                for (int i = 0; i < zipFiles.Length; i++)
                {
                    FileInfo fi = new FileInfo(zipFiles[i]);

                    string entryName = Path.GetFileName(zipFiles[i]);
                    entryName = ZipEntry.CleanName(entryName);
                    ZipEntry newEntry = new ZipEntry(entryName);
                    newEntry.DateTime = fi.LastWriteTime;

                    zipStream.PutNextEntry(newEntry);
                    byte[] buffer = new byte[4096];

                    using (FileStream streamReader = File.OpenRead(zipFiles[i]))
                    {
                        StreamUtils.Copy(streamReader, zipStream, buffer);
                    }
                }
                zipStream.CloseEntry();
            }
        }
    }
}
