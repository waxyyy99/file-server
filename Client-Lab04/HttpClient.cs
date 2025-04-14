using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<string> ReadString(NetworkStream ns)
        {
            byte[] conentLengthBuffer = new byte[4];
            int bytesRead = await ns.ReadAsync(conentLengthBuffer, 0, 4);
            int contentLength = BitConverter.ToInt32(conentLengthBuffer, 0);

            byte[] buffer = new byte[contentLength];
            bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public static async Task WriteString(NetworkStream ns, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            var contentLengthByte = BitConverter.GetBytes(bytes.Length);

            await ns.WriteAsync(contentLengthByte, 0, contentLengthByte.Length);
            await ns.WriteAsync(bytes, 0, bytes.Length);
        }

        public async Task StartUp(string[] args)
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Enter action (1 - get a file, 2 - create a file, 3 - delete a file, exit - to exit):");
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
                                string input = Console.ReadLine() ?? "";
                                if (string.IsNullOrEmpty(input))
                                {
                                    continue;
                                }
                                query += input;

                                await WriteString(Stream, query);
                                Console.WriteLine("The request was sent.");

                                string resp = await ReadString(Stream);
                                if (resp.Split()[0] == "200")
                                {
                                    Console.Write("The content of the file is: " + resp[4..]);
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
                        case "2":
                            {
                                query += "PUT ";
                                Console.WriteLine("Enter filename: ");
                                string fileName = Console.ReadLine();
                                if (string.IsNullOrEmpty(fileName))
                                {
                                    continue;
                                }
                                Console.WriteLine("Enter file content: ");
                                string fileContent = Console.ReadLine();
                                if (string.IsNullOrEmpty(fileContent))
                                {
                                    continue;
                                }
                                query += fileName + ' ' + fileContent;

                                await WriteString(Stream, query);
                                Console.WriteLine("The request was sent.");

                                string resp = await ReadString(Stream);
                                if (resp == "200")
                                {
                                    Console.WriteLine("The response says that the file was successfully created!");
                                }
                                else if (resp == "403")
                                {
                                    Console.WriteLine("The response says forbidden to create this file!");
                                }
                                else
                                {
                                    Console.WriteLine("Error code: " + resp);
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

                                await WriteString(Stream, query);
                                Console.WriteLine("The request was sent.");

                                string resp = await ReadString(Stream);
                                if (resp == "200")
                                {
                                    Console.WriteLine("The response says that the file was successfully deleted!");
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
