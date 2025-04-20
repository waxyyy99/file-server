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
        public TcpClient Client { get; }
        public NetworkStream Stream => Client.GetStream();

        public Guid Id { get; } = Guid.NewGuid();
        public string? Username { get; set; }


        public ClientSession(TcpClient client)
        {
            Client = client;
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
                        case "PUT":
                            {
                                try
                                {
                                    var fileName = _[1];
                                    int firstSpace = recievedMessage.IndexOf(' ');
                                    int secondSpaceInd = recievedMessage.IndexOf(' ', firstSpace + 1);
                                    var text = recievedMessage[(secondSpaceInd + 1)..];
                                    await ServerService.PutFile(fileName, text);

                                    response = new(200, null);
                                }
                                catch
                                {
                                    response = new(403, null);
                                }
                                break;
                            }
                        case "DELETE":
                            {
                                try
                                {
                                    var fileName = _[1];
                                    ServerService.DeleteFile(fileName);
                                    response = new(200, null);

                                }
                                catch
                                {
                                    response = new(404, null);
                                }
                                break;
                            }
                        case "GET":
                            {
                                try
                                {
                                    var fileName = recievedMessage.Split()[1];
                                    var content = await ServerService.GetFile(fileName);
                                    response = new(200, content);
                                }
                                catch
                                {
                                    response = new(404, null);
                                }
                                break;
                            }
                        default:
                            response = new(400, null);
                            break;
                    }

                    var responseString = $"{response.StatusCode}";
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        responseString += ' ' + response.Content;
                    }
                    Console.WriteLine("[server] Send: " + responseString);
                    await Stream.WriteStringAsync(responseString);
                }
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
