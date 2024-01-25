using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ProtoBuf;

namespace DesignAuditRise.Service.Utility
{
    public static class FileProcessUtils
    {
        public static List<T> LoadStream<T>(Stream stream) where T : class
        {
            if (stream != null && stream.Length != 0)
            {
                return Serializer.Deserialize<List<T>>(stream);
            }
            else
            {
                return new List<T>();
            }
        }

        public static void zipFiles(string savePath, string[] zipFiles)
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
