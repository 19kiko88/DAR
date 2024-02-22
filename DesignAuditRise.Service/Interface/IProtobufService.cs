using Microsoft.AspNetCore.Mvc;
using ProtoBuf;
using System.Data;

namespace DesignAuditRise.Service.Interface
{
    public interface IProtobufService
    {
        public Task<List<T>> ProtobufDatFileToList<T>(string datFilePath);

        public Task SaveProtobufDatFile<T>(string savePath, List<T> data);

        //public Task<List<T>> LoadStream<T>(Stream stream) where T : class;
    }
}
