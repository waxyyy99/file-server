using Server;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public class HttpClient
    {
        private TcpClient client { get; init; }
        NetworkStream Stream => client.GetStream();
        public IPAddress Ip { get; init; }
        public int Port { get; init; }

        public HttpClient(IPAddress ip, int port) 
        {
            Ip = ip;
            Port = port;

            try
            {
                client = new TcpClient(Ip.ToString(), Port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка клиента: {0}", ex.Message);
            }
        }

        public async Task StartUp(string[] args)
        {
            try
            {
                while (true)
                {
                    using var cts = new CancellationTokenSource();
                    Console.WriteLine("Enter action (1 - get a file from server, 2 - copy file to server, 3 - delete a file, exit - to exit):");
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
                    switch (command)
                    {
                        case "1":
                            {

                                query += "GET ";
                                Console.WriteLine("Enter filename: ");
                                string getFileName = Console.ReadLine() ?? "";
                                if (string.IsNullOrEmpty(getFileName))
                                {
                                    continue;
                                }
                                Console.WriteLine("Enter saved file: ");
                                string getSavedFileName = Console.ReadLine() ?? "";
                                if (string.IsNullOrEmpty(getSavedFileName))
                                {
                                    continue;
                                }

                                query += getFileName;

                                await Stream.WriteStringAsync(query);
                                Console.WriteLine("The request was sent.");

                                string resp = await Stream.ReadStringAsync();
                                if (resp.Split()[0] == "200")
                                {
                                    FileInfo file = new FileInfo(getSavedFileName);
                                    await Stream.ReadFileAsync(file, cts.Token);
                                    Console.WriteLine($"File {getSavedFileName} saved localy");

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
                                var resp = (await Stream.ReadStringAsync()).Split();
                                var statusCode = resp[0];
                                if (statusCode == "200")
                                {
                                    var fileId = resp[1];
                                    Console.WriteLine($"The response says that the file can be successfuly copied! File id is: {fileId}");
                                    Console.WriteLine("File upload started!");
                                    await Stream.WriteFileAsync(file, cts.Token);
                                    Console.WriteLine("File upload ended!");
                                }
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
                                Console.WriteLine("Enter filename: ");
                                string input = Console.ReadLine();
                                if (string.IsNullOrEmpty(input))
                                {
                                    continue;
                                }
                                query += input;

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
