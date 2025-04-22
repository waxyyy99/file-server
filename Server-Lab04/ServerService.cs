using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    public class ServerService : IDisposable
    {
        //public Dictionary<int, string> nameId;
        private readonly FileIdProvider _fileIdProvider;
        public ServerService()
        {
            const string fileName = "pairs.json";
            FileInfo fileInfo = new(fileName);
            _fileIdProvider = new(fileInfo);
            //using var file = File.OpenRead(fileName);
            //nameId = JsonSerializer.Deserialize<Dictionary<int, string>>(file)!;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
            return (fileInfo, _fileIdProvider.AppendFile(fileName));
        }

        public void DeleteFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception($"Have no file: {fileName}");
            }
            File.Delete(fileName);
            _fileIdProvider.DeleteFile(fileName);
        }
        public void DeleteFile(int fileId)
        {
            DeleteFile(_fileIdProvider.GetFileName(fileId));
        }

        public void Dispose()
        {
            _fileIdProvider.Dispose();
        }
    }
}
