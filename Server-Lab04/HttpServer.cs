using Server.RequestHandle;
using Server.RequestHandle.Commands;
using Server.Services;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public class HttpServer
    {
        private readonly ServerService _serverService;
        private readonly RequestHandlerFactory _handlerFactory;
        public IPAddress Ip { get; init; }
        public int Port { get; init; }

        public HttpServer(IPAddress ip, int port) {
            _serverService = new ServerService();
            _handlerFactory = new(
            [
                new DeleteHandler(),
                new GetHandler(),
                new PutHandler()
            ]);
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

                    // Принимаем клиента
                    TcpClient client = await server.AcceptTcpClientAsync(cts.Token);
                    ClientSession session = new(client, _serverService, _handlerFactory);
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
