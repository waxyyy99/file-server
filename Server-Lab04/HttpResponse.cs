using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public record class HttpResponse(int StatusCode, string? Content)
    {
        public async Task Send(NetworkStream stream)
        {
            var responseString = $"{StatusCode}";
            if (!string.IsNullOrEmpty(Content))
            {
                responseString += ' ' + Content;
            }
            Console.WriteLine("[server] Send: " + responseString);
            await stream.WriteStringAsync(responseString);
        }
    }
}
