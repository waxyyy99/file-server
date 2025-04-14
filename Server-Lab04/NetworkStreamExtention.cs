using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public static class NetworkStreamExtention
    {
        public static async Task<string> ReadAsync(this NetworkStream ns)
        {
            byte[] conentLengthBuffer = new byte[4];
            int bytesRead = await ns.ReadAsync(conentLengthBuffer, 0, 4);
            int contentLength = BitConverter.ToInt32(conentLengthBuffer, 0);

            byte[] buffer = new byte[contentLength];
            bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public static async Task WriteAsync(this NetworkStream ns, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            var contentLengthByte = BitConverter.GetBytes(bytes.Length);

            await ns.WriteAsync(contentLengthByte, 0, contentLengthByte.Length);
            await ns.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
