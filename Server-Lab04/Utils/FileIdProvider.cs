using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Server.Utils
{
    public class DictionaryCounter<TValue>
    {
        [JsonInclude]
        public Dictionary<int, TValue> Items { get; private set; }

        [JsonInclude]                               
        public int LastId { get; private set; }
        private int NextId => ++LastId;

        public DictionaryCounter() {
            Items = new();
        }

        public void Add(TValue value, out int id)
        {
            id = NextId;
            Items.Add(id, value);
        }
    }


    public class FileIdProvider : IDisposable
    {
        protected readonly DictionaryCounter<string> nameIdPair;
        protected readonly FileInfo _jsonFile;
        public FileIdProvider(FileInfo jsonFile)
        {
            _jsonFile = jsonFile;
            if (!_jsonFile.Exists)
            {
                nameIdPair = new DictionaryCounter<string>();
                
                return;
            }
            using var file = _jsonFile.OpenRead();
            nameIdPair = JsonSerializer.Deserialize<DictionaryCounter<string>>(file) ?? new();
        }
        public void Commit()
        {
            using var file = _jsonFile.Open(FileMode.Create);
            JsonSerializer.Serialize(file, nameIdPair);
        }
        public void Dispose()
        {
            Commit();
        }

        public bool ContainsFile(string fileName)
        {
            return nameIdPair.Items.ContainsValue(fileName);
        }
        public bool ContainsFile(int fileId)
        {
            return nameIdPair.Items.ContainsKey(fileId);
        }
        public string GetFileName(int fileId)
        {
            return nameIdPair.Items[fileId];
        }
        public int GetFileId(string fileName)
        {
            foreach (var item in nameIdPair.Items)
            {
                if (item.Value == fileName)
                {
                    return item.Key;
                }
            }
            throw new FileNotFoundException();
        }
        /// <summary>
        /// Returns id of appended file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int AppendFile(string fileName)
        {
            int id;
            nameIdPair.Add(fileName, out id);
            return id;
        }
        public void DeleteFile(int fileId)
        {
            if (!nameIdPair.Items.Remove(fileId))
            {
                throw new InvalidOperationException($"File with id {fileId} doesn't exists!");
            }
        }
        public void DeleteFile(string fileName)
        {
            bool finded = false;
            foreach (var item in nameIdPair.Items)
            {
                if (item.Value == fileName)
                {
                    finded = true;
                    nameIdPair.Items.Remove(item.Key);
                    break;
                }
            }
            if (!finded)
            {
                throw new InvalidOperationException($"File {fileName} doesn't exists!");
            }
        }
    }
}
