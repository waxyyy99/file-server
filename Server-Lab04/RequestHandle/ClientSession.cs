using Server.Services;
using Server.Utils;
using System.Net.Sockets;

namespace Server.RequestHandle
{
    public class ClientSession
    {
        private readonly ServerService _serverService;
        private readonly RequestHandlerFactory _handlerFactory;
        public CancellationTokenSource cts;
        public TcpClient Client { get; }
        public NetworkStream Stream => Client.GetStream();

        public Guid Id { get; } = Guid.NewGuid();
        public string? Username { get; set; }



        public ClientSession(TcpClient client, ServerService service, RequestHandlerFactory handlerFactory)
        {
            cts = new CancellationTokenSource();
            Client = client;
            _serverService = service;
            _handlerFactory = handlerFactory;
        }

        public async Task HandleAsync()
        {
            Console.WriteLine("[server] Task {0} start", Task.CurrentId!.Value);
            Console.WriteLine("[server] Client connected!");


            try
            {
                while (true)
                {
                    var recievedMessage = await Stream.ReadStringAsync();
                    Console.WriteLine("[server] Recieved: {0}", recievedMessage);

                    var handler = _handlerFactory.GetCommandHandler(recievedMessage);
                    await handler.HandleAsync(recievedMessage, this, _serverService, cts.Token);
                }
            }
            catch (OperationCanceledException) 
            {
                Console.WriteLine("[server] Operation breaked");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[server] Client operation failed: {0}", ex.Message);
            }
            finally
            {
                cts.Cancel();
                Client.Close();
                Console.WriteLine("[server] User disconnected");
            }
        }
    }
}
