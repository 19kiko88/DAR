using ClosedXML.Excel;
using DesignAuditRise.Service.Interface;
using DesignAuditRise.Web;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOI.HPSF;
using ProtoBuf;
using System.Data;
using System.Drawing.Text;
using System.IO;
using System.Text;

namespace DesignAuditRise.Service.Implement
{
    public class ProtobufService: IProtobufService
    {
        public async Task<List<T>> ProtobufDatFileToList<T>(string datFilePath)
        {
            var bytes = File.ReadAllBytes(datFilePath);

            using (var ms = new MemoryStream(bytes, 0, bytes.Length, true, true))
            {
                if (ms != null && ms.Length != 0)
                {
                    return Serializer.Deserialize<List<T>>(ms);
                }
                else
                {
                    return new List<T>();
                }
            }
        }

        public async Task SaveProtobufDatFile<T>(string savePath, List<T> data)
        {
            using (var fs = File.Create(savePath))
            {
                //序列化回新的filter_part.dat檔
                Serializer.Serialize(fs, data);
            }
        }
        //public async Task<List<T>> LoadStream<T>(Stream stream) where T : class
        //{
        //    if (stream != null && stream.Length != 0)
        //    {
        //        return Serializer.Deserialize<List<T>>(stream);
        //    }
        //    else
        //    {
        //        return new List<T>();
        //    }
        //}
    }
}
