using System.Net;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var client = new HttpClient(IPAddress.Parse("127.0.0.1"), 3000);
            await client.StartUp(args);
        }
    }
}
