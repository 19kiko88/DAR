using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ProtoBuf;

namespace DesignAuditRise.Service.Utility
{
    public static class ZipUtils
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
    }
}
