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
        private readonly ServerService _serverService;
        public static uint AmountOfSolvedEquations { get; private set; } = 0;
        public static Dictionary<int, User> UserTask { get; private set; } = new Dictionary<int, User>();
        public IPAddress Ip { get; init; }
        public int Port { get; init; }

        public HttpServer(IPAddress ip, int port) {
            _serverService = new ServerService();
            Ip = ip;
            Port = port;
        }

        public async Task StartServer() 
        {
            TcpListener server = new TcpListener(Ip, Port);
            CancellationTokenSource cts = new();

            try
            {
                server.Start();
                Console.WriteLine("[server] Сервер запущен на {0}:{1}", Ip, Port);

                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        var input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input))
                        {
                            continue;
                        }
                        if (input == "stop")
                        {
                            cts.Cancel();
                        }
                    }
                });

                while (!cts.IsCancellationRequested)
                {
                    Console.WriteLine("[server] Ожидание подключения...");
                    //Thread.Sleep(10000);

                    // Принимаем клиента
                    TcpClient client = await server.AcceptTcpClientAsync(cts.Token);
                    ClientSession session = new(client, _serverService);
                    _ = Task.Run(session.HandleAsync);
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("[server] Server stoped");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[server] Server error: {0}", ex.Message);
            }
            finally
            {
                _serverService.Dispose();
                server.Stop();
            }
        }
    }
}
