using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.RequestHandle.Commands.Interface
{
    public interface ICommandHandler
    {
        bool CanHandle(string command);
        
        Task HandleAsync(string command, ClientSession session, ServerService _serverService, CancellationToken token = default);
    }
}
