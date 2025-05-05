using Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.Services
{
    public class ServerService : IDisposable
    {
        private readonly FileIdProvider _fileIdProvider;
        public ServerService()
        {
            const string fileName = "pairs.json";
            FileInfo fileInfo = new(fileName);
            _fileIdProvider = new(fileInfo);
        }
        public bool ContainsFile(string fileName) => _fileIdProvider.ContainsFile(fileName);
        public bool ContainsFile(int fileId) => _fileIdProvider.ContainsFile(fileId);

        public FileInfo GetFile(string fileName)
        {
            string[] path = ["server", "data", fileName];
            var universalPath = Path.Combine(path);
            var fileInfo = new FileInfo(universalPath);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException($"{universalPath} does not exists");
            }
            return fileInfo;
        }
        public FileInfo GetFile(int fileId) => GetFile(_fileIdProvider.GetFileName(fileId));

        public (FileInfo, int) CreateFile(string fileName)
        {
            string[] path = ["server", "data", fileName];
            var universalPath = Path.Combine(path);
            var fileInfo = new FileInfo(universalPath);
            if (fileInfo.Exists)
            {
                throw new ArgumentException($"{universalPath} already exists.");
            }
            
            fileInfo.Create().Close();
            var x = (fileInfo, _fileIdProvider.AppendFile(fileName));
            _fileIdProvider.Commit();
            return x;
        }

        public void DeleteFile(string fileName)
        {
            string[] path = ["server", "data", fileName];
            var universalPath = Path.Combine(path);
            var fileInfo = new FileInfo(universalPath);
            if (!fileInfo.Exists)
            {
                throw new Exception($"Have no file: {fileName}");
            }
            fileInfo.Delete();
            _fileIdProvider.DeleteFile(fileName);
            _fileIdProvider.Commit();
        }
        public void DeleteFile(int fileId)
        {
            var fileName = _fileIdProvider.GetFileName(fileId); string[] path = ["server", "data", fileName];
            var universalPath = Path.Combine(path);
            var fileInfo = new FileInfo(universalPath);
            if (!fileInfo.Exists)
            {
                throw new Exception($"Have no file: {fileName}");
            }
            fileInfo.Delete();
            _fileIdProvider.DeleteFile(fileId);
            _fileIdProvider.Commit();
        }

        public void Dispose()
        {
            _fileIdProvider.Dispose();
        }
    }
}
