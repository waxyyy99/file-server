using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ServerService
    {
        public static async Task<string> GetFile(string fileName)
        {
            string strContent;
            using (var file = File.OpenRead(fileName))
            using (var sr = new StreamReader(file))
            {
                strContent = await sr.ReadToEndAsync();
            }
            return strContent;
        }
        public static async Task PutFile(string fileName, string text)
        {

            using (var file = File.Open(fileName, FileMode.CreateNew, FileAccess.Write))
            using (var sw = new StreamWriter(file))
            {
                await sw.WriteAsync(text);
            }
        }

        public static void DeleteFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception($"Have no file: {fileName}");
            }
            File.Delete(fileName);
        }
    }
}
