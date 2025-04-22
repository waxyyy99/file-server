using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ClientSession
    {
        private readonly ServerService _serverService;
        public TcpClient Client { get; }
        public NetworkStream Stream => Client.GetStream();

        public Guid Id { get; } = Guid.NewGuid();
        public string? Username { get; set; }


        public ClientSession(TcpClient client, ServerService service)
        {
            Client = client;
            _serverService = service;
        }

        public async Task HandleAsync()
        {
            Console.WriteLine("[server] Task {0} start", Task.CurrentId!.Value);
            Console.WriteLine("[server] Client connected!");
            try
            {
                while (true)
                {
                    using var cts = new CancellationTokenSource();

                    var recievedMessage = await Stream.ReadStringAsync();
                    Console.WriteLine("[server] Recieved: {0}", recievedMessage);

                    var _ = recievedMessage.Split();

                    var command = _.FirstOrDefault();
                    if (command is null)
                    {
                        Console.WriteLine("[server] Can't find querry type!");
                        throw new Exception("Can't find querry type!");
                    }

                    HttpResponse response;

                    switch (command)
                    {
                        // PUT {fileName}
                        // binary content
                        case "PUT":
                            {
                                try
                                {
                                    var fileName = _[1];

                                    //int firstSpace = recievedMessage.IndexOf(' ');
                                    //int secondSpaceInd = recievedMessage.IndexOf(' ', firstSpace + 1);
                                    //var text = recievedMessage[(secondSpaceInd + 1)..];
                                    var fileId = _serverService.CreateFile(fileName);
                                    response = new(200, fileId.Item2.ToString());
                                    await response.Send(Stream);

                                    await Stream.ReadFileAsync(fileId.Item1, cts.Token);
                                }
                                catch
                                {
                                    response = new(403, null);
                                    await response.Send(Stream);
                                }
                                break;
                            }
                        // DELETE [BY_ID | BY_NAME] [ID | NAME]
                        case "DELETE":
                            {
                                try
                                {
                                    var method = _[1];
                                    var fileName = _[2];

                                    _serverService.DeleteFile(fileName);
                                    response = new(200, null);
                                    await response.Send(Stream);
                                }
                                catch
                                {
                                    response = new(404, null);
                                    await response.Send(Stream);
                                }
                                break;
                            }
                        // GET [BY_ID | BY_NAME] [ID | NAME]
                        case "GET":
                            {
                                FileInfo content;
                                try
                                {
                                    var fileName = recievedMessage.Split()[1];
                                    content = _serverService.GetFile(fileName);
                                    response = new(200, null);
                                    await response.Send(Stream);

                                    await Stream.WriteFileAsync(content, cts.Token);
                                    Console.WriteLine("[server] Send file: " + content.Name);
                                }
                                catch
                                {
                                    response = new(404, null);
                                    await response.Send(Stream);
                                    break;
                                }
                                break;
                            }
                        default:
                            response = new(400, null);
                            await response.Send(Stream);
                            break;
                    }

                    
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
                Client.Close();
                Console.WriteLine("[server] User disconnected");
            }
        }
    }
}
