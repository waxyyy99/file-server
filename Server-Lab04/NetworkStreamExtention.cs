using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class NetworkStreamExtention
    {
        public static async Task<string> ReadStringAsync(this NetworkStream ns)
        {
            byte[] conentLengthBuffer = new byte[4];
            await ns.ReadExactlyAsync(conentLengthBuffer);
            int contentLength = BitConverter.ToInt32(conentLengthBuffer, 0);

            byte[] buffer = new byte[contentLength];
            int bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public static async Task WriteStringAsync(this NetworkStream ns, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            var contentLengthByte = BitConverter.GetBytes(bytes.Length);
            await ns.WriteAsync(contentLengthByte, 0, contentLengthByte.Length);
            await ns.WriteAsync(bytes, 0, bytes.Length);
        }

        public static async Task WriteFileAsync(this NetworkStream ns, FileInfo fileInfo, CancellationToken cancellationToken)
        {
            const int bufferSize = 81920;
            var fileLength = fileInfo.Length;
            Console.WriteLine($"File length: {fileLength}");

            await ns.WriteAsync(BitConverter.GetBytes(fileLength), 0, 8, cancellationToken);

            using var file = fileInfo.OpenRead();
            await file.CopyToAsync(ns, bufferSize, cancellationToken);
        }
        public static async Task ReadFileAsync(this NetworkStream ns, FileInfo fileInfo, CancellationToken cancellationToken)
        {
            byte[] length = new byte[8];
            await ns.ReadExactlyAsync(length, cancellationToken);
            long fileLength = BitConverter.ToInt64(length, 0);
            Console.WriteLine($"File length: {fileLength}");

            const int bufferSize = 81920;
            byte[] buffer = new byte[bufferSize];
            long recieved = 0;

            using var fs = fileInfo.OpenWrite();
            while (recieved < fileLength)
            {
                int toRead = (int)Math.Min(fileLength - recieved, bufferSize);
                int read = await ns.ReadAsync(buffer, 0, toRead, cancellationToken);
                if (read == 0) throw new IOException("Сокет преждевременно закрыл соединение.");
                await fs.WriteAsync(buffer, 0, read, cancellationToken);
                recieved += read;
                Console.WriteLine(recieved);
            }
        }
    }
}
