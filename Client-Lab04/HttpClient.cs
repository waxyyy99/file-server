using Server;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public class HttpClient
    {
        private TcpClient client { get; set; }
        NetworkStream Stream => client.GetStream();
        public IPAddress Ip { get; init; }
        public int Port { get; init; }

        public HttpClient(IPAddress ip, int port) 
        {
            Ip = ip;
            Port = port;

        }

        public async Task StartUp(string[] args)
        {
            try
            {
                client = new TcpClient(Ip.ToString(), Port);
                while (true)
                {
                    using var cts = new CancellationTokenSource();
                    Console.WriteLine("Enter action (1 - get a file from server, 2 - copy file to server, 3 - delete a file, rec - to reconnect, exit - to exit):");
                    string? command = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(command))
                    {
                        Console.WriteLine("Пустая команда. Повторите.");
                        continue;
                    }

                    command = command.ToLower();
                    string query = "";

                    if (command == "exit")
                    {
                        break;
                    }
                    else if(command == "rec")
                    {
                        client.Close();
                        client = new TcpClient(Ip.ToString(), Port);
                    }
                    switch (command)
                    {
                        case "1":
                            {

                                query += "GET ";
                                Console.WriteLine("Do you want to get the file by name or by id (1 - name, 2 - id):");
                                string? getMethod = Console.ReadLine();

                                if (string.IsNullOrWhiteSpace(getMethod))
                                {
                                    Console.WriteLine("Empty input! Please try again");
                                    continue;
                                }

                                if (getMethod == "1")
                                {
                                    Console.WriteLine("Enter file name: ");
                                    string getFileName = Console.ReadLine() ?? "";
                                    if (string.IsNullOrEmpty(getFileName))
                                    {
                                        continue;
                                    }
                                    query += "BY_NAME ";
                                    query += getFileName;

                                }
                                else if (getMethod == "2")
                                {
                                    Console.WriteLine("Enter file id: ");
                                    string getFileId = Console.ReadLine() ?? "";
                                    if (string.IsNullOrEmpty(getFileId))
                                    {
                                        continue;
                                    }
                                    query += "BY_ID ";
                                    query += getFileId;
                                }
                                else
                                {
                                    Console.WriteLine("Unknown command! Please try again");
                                    continue;
                                }

                                Console.WriteLine("Enter saved file: ");
                                string getSavedFileName = Console.ReadLine() ?? "";
                                if (string.IsNullOrEmpty(getSavedFileName))
                                {
                                    continue;
                                }


                                await Stream.WriteStringAsync(query);
                                Console.WriteLine("The request was sent.");

                                string resp = await Stream.ReadStringAsync();
                                // If server send 200 read file
                                if (resp.Split()[0] == "200")
                                {
                                    FileInfo file = new FileInfo(getSavedFileName);
                                    await Stream.ReadFileAsync(file, cts.Token);
                                    Console.WriteLine($"File {getSavedFileName} saved localy");

                                }
                                // else
                                else if (resp == "404")
                                {
                                    Console.WriteLine("The response says that the file not found!");
                                }
                                else
                                {
                                    Console.WriteLine("Error code: " + resp);
                                }

                                break;
                                throw new NotImplementedException();
                            }
                        case "2":
                            {
                                query += "PUT ";
                                Console.WriteLine("Enter filename: ");
                                string fileName = Console.ReadLine();
                                if (string.IsNullOrEmpty(fileName))
                                {
                                    Console.WriteLine("Input is empty!");
                                    continue;
                                }
                                Console.WriteLine("Enter name of file on server: ");
                                string serverFileName = Console.ReadLine();
                                if (string.IsNullOrEmpty(serverFileName))
                                {
                                    Console.WriteLine("Input is empty!");
                                    continue;
                                }

                                FileInfo file = new(fileName);
                                if (!file.Exists)
                                {
                                    Console.WriteLine("File doesn't exists!");
                                    continue;
                                }

                                query += serverFileName;

                                await Stream.WriteStringAsync(query);
                                Console.WriteLine("The request was sent.");
                                // request for upload file on server
                                var resp = (await Stream.ReadStringAsync()).Split();
                                var statusCode = resp[0];

                                // request approved
                                if (statusCode == "200")
                                {
                                    var fileId = resp[1];
                                    Console.WriteLine($"The response says that the file can be successfuly copied! File id is: {fileId}");
                                    Console.WriteLine("File upload started!");
                                    await Stream.WriteFileAsync(file, cts.Token);
                                    Console.WriteLine("File upload ended!");
                                }
                                // request not approved
                                else if (statusCode == "403")
                                {
                                    Console.WriteLine("The response says forbidden to copy this file!");
                                }
                                else
                                {
                                    Console.WriteLine("Error code: " + statusCode);
                                }
                                break;
                            }
                        case "3":
                            {
                                query += "DELETE ";

                                Console.WriteLine("Do you want to delete the file by name or by id (1 - name, 2 - id):");
                                string? deleteMethod = Console.ReadLine();

                                if (string.IsNullOrWhiteSpace(deleteMethod))
                                {
                                    Console.WriteLine("Empty input! Please try again");
                                    continue;
                                }

                                if (deleteMethod == "1")
                                {
                                    Console.WriteLine("Enter filename: ");
                                    string getFileName = Console.ReadLine() ?? "";
                                    if (string.IsNullOrEmpty(getFileName))
                                    {
                                        continue;
                                    }
                                    query += "BY_NAME ";
                                    query += getFileName;

                                }
                                else if (deleteMethod == "2")
                                {
                                    Console.WriteLine("Enter file id: ");
                                    string getFileId = Console.ReadLine() ?? "";
                                    if (string.IsNullOrEmpty(getFileId))
                                    {
                                        continue;
                                    }
                                    query += "BY_ID ";
                                    query += getFileId;
                                }
                                else
                                {
                                    Console.WriteLine("Unknown command! Please try again");
                                    continue;
                                }

                                await Stream.WriteStringAsync(query);
                                Console.WriteLine("The request was sent.");

                                string resp = await Stream.ReadStringAsync();
                                if (resp == "200")
                                {
                                    Console.WriteLine("The response says that the file was successfully deleted from server!");
                                }
                                else if (resp == "404")
                                {
                                    Console.WriteLine("The response says that the file not found!");
                                }
                                else
                                {
                                    Console.WriteLine("Error code: " + resp);
                                }
                                break;
                            }
                        default:
                            continue;
                    }
                }
            }
            finally
            {
                client.Close();
                Console.WriteLine("Соединение закрыто");
            }
        }
    }
}
