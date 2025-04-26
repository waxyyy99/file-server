using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ClientSession
    {
        private readonly ServerService _serverService;
        private readonly CancellationTokenSource cts;
        public TcpClient Client { get; }
        public NetworkStream Stream => Client.GetStream();

        public Guid Id { get; } = Guid.NewGuid();
        public string? Username { get; set; }



        public ClientSession(TcpClient client, ServerService service)
        {
            cts = new CancellationTokenSource();
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
                                (FileInfo, int) fileId;
                                try
                                {
                                    var fileName = _[1];

                                    fileId = _serverService.CreateFile(fileName);
                                    // send id of created file
                                    response = new(200, fileId.Item2.ToString());
                                    await response.Send(Stream);

                                    await Stream.ReadFileAsync(fileId.Item1, cts.Token);
                                }
                                catch(IOException ex)
                                {
                                    throw new IOException("Can't read file from stream", ex);
                                }
                                catch(Exception ex)
                                {
                                    // request denied
                                    response = new(403, null);
                                    await response.Send(Stream);
                                    break;
                                }
                                break;
                            }
                        // DELETE [BY_ID | BY_NAME] [ID | NAME]
                        case "DELETE":
                            {
                                try
                                {
                                    var findMethod = _[1];
                                    var fileFindCredentials = _[2];

                                    if (findMethod == "BY_ID")
                                    {
                                        int id;
                                        if (!int.TryParse(fileFindCredentials, out id))
                                        {
                                            throw new ArgumentException("Can't parse id");
                                        }
                                        _serverService.DeleteFile(id);
                                    }
                                    else if (findMethod == "BY_NAME")
                                    {
                                        _serverService.DeleteFile(fileFindCredentials);
                                    }
                                    else
                                    {
                                        throw new ArgumentException();
                                    }

                                    response = new(200, null);
                                    await response.Send(Stream);
                                }
                                catch (ArgumentException ex)
                                {
                                    response = new(400, null);
                                    await response.Send(Stream);
                                    break;
                                }
                                catch
                                {
                                    response = new(404, null);
                                    await response.Send(Stream);
                                    break;
                                }
                                break;
                            }
                        // GET [BY_ID | BY_NAME] [ID | NAME]
                        case "GET":
                            {
                                try
                                {
                                    FileInfo content;
                                    var findMethod = _[1];
                                    var fileFindCredentials = _[2];
                                    if (findMethod == "BY_ID")
                                    {
                                        int id;
                                        if (!int.TryParse(fileFindCredentials, out id))
                                        {
                                            throw new ArgumentException("Can't parse id");
                                        }
                                        content = _serverService.GetFile(id);
                                    }
                                    else if (findMethod == "BY_NAME")
                                    {
                                        content = _serverService.GetFile(fileFindCredentials);
                                    }
                                    else
                                    {
                                        throw new ArgumentException();
                                    }
                                    response = new(200, null);
                                    await response.Send(Stream);

                                    await Stream.WriteFileAsync(content, cts.Token);
                                    Console.WriteLine("[server] Send file: " + content.Name);
                                }
                                catch (ArgumentException ex)
                                {
                                    response = new(400, null);
                                    await response.Send(Stream);
                                    break;
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
                cts.Cancel();
                Client.Close();
                Console.WriteLine("[server] User disconnected");
            }
        }
    }
}
