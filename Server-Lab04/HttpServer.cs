using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class HttpServer
    {
        public static uint AmountOfSolvedEquations { get; private set; } = 0;
        public static Dictionary<int, User> UserTask { get; private set; } = new Dictionary<int, User>();
        public IPAddress Ip { get; init; }
        public int Port { get; init; }

        public HttpServer(IPAddress ip, int port) {
            Ip = ip;
            Port = port;
        } 
         
        //public static async Task<string> ReadString(NetworkStream ns)
        //{
        //    byte[] conentLengthBuffer = new byte[4];
        //    int bytesRead = await ns.ReadAsync(conentLengthBuffer, 0, conentLengthBuffer.Length);
        //    int contentLength = BitConverter.ToInt32(conentLengthBuffer, 0);

        //    byte[] buffer = new byte[contentLength];
        //    bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);

        //    string str = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //    return str;
        //}

        //public static async Task WriteString(NetworkStream ns, string message)
        //{
        //    var contentLengthByte = BitConverter.GetBytes(message.Length);
        //    await ns.ReadAsync(contentLengthByte, 0, contentLengthByte.Length);

        //    byte[] bytes = Encoding.UTF8.GetBytes(message);
        //    await ns.ReadAsync(bytes, 0, bytes.Length);
        //}

        public async Task StartServer() 
        {
            TcpListener server = new TcpListener(Ip, Port);

            try
            {
                server.Start();
                Console.WriteLine("[server] Сервер запущен на {0}:{1}", Ip, Port);

                while (true)
                {
                    Console.WriteLine("[server] Ожидание подключения...");
                    //Thread.Sleep(10000);

                    // Принимаем клиента
                    TcpClient client = await server.AcceptTcpClientAsync();
                    ClientSession session = new(client);
                    _ = Task.Run(session.HandleAsync);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[server] Server error: {0}", ex.Message);
            }
            finally
            {
                server.Stop();
            }
        }
    }
}
