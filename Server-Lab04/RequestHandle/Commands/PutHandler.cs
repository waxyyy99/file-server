using Server.RequestHandle.Commands.Interface;
using Server.Services;
using Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.RequestHandle.Commands
{
    internal class PutHandler : ICommandHandler
    {
        public bool CanHandle(string command) => command.StartsWith("PUT", StringComparison.OrdinalIgnoreCase);

        public async Task HandleAsync(string command, ClientSession session, ServerService _serverService, CancellationToken token = default)
        {
            var _ = command.Split();
            (FileInfo, int) fileId;
            HttpResponse response;
            try
            {
                var fileName = _[1];

                fileId = _serverService.CreateFile(fileName);
                // send id of created file
                response = new(200, fileId.Item2.ToString());
                await response.Send(session.Stream);

                await session.Stream.ReadFileAsync(fileId.Item1, session.cts.Token);
            }
            catch (IOException ex)
            {
                throw new IOException("Can't read file from stream", ex);
            }
            catch (Exception ex)
            {
                // request denied
                response = new(403, null);
                await response.Send(session.Stream);
            }
        }
    }
}
