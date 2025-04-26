using Server.RequestHandle.Commands.Interface;
using Server.Services;
using Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.RequestHandle.Commands
{
    internal class GetHandler : ICommandHandler
    {
        public bool CanHandle(string command) => command.StartsWith("GET", StringComparison.OrdinalIgnoreCase);

        public async Task HandleAsync(string command, ClientSession session, ServerService _serverService, CancellationToken token = default)
        {
            HttpResponse response;

            var _ = command.Split();
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
                await response.Send(session.Stream);

                await session.Stream.WriteFileAsync(content, session.cts.Token);
                Console.WriteLine("[server] Send file: " + content.Name);
            }
            catch (ArgumentException ex)
            {
                response = new(400, null);
                await response.Send(session.Stream);
            }
            catch
            {
                response = new(404, null);
                await response.Send(session.Stream);
            }
        }
    }
}
