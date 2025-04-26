using Server.RequestHandle.Commands.Interface;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.RequestHandle.Commands
{
    internal class DeleteHandler : ICommandHandler
    {
        public bool CanHandle(string command) => command.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase);

        public async Task HandleAsync(string command, ClientSession session, ServerService _serverService, CancellationToken token = default)
        {
            var _ = command.Split();
            HttpResponse response;

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
                await response.Send(session.Stream);
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
