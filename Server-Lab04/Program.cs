using System.Net;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            HttpServer server = new HttpServer(IPAddress.Parse("127.0.0.1"), 3000);
            await server.StartServer();    
            
        }
    }
}
