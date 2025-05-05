using Server.RequestHandle.Commands.Interface;

namespace Server.RequestHandle
{
    public class RequestHandlerFactory
    {
        private readonly IEnumerable<ICommandHandler> _handlers;
        public RequestHandlerFactory(IEnumerable<ICommandHandler> handlers)
        {
            _handlers = handlers;
        }

        public ICommandHandler GetCommandHandler(string command)
        {
            var handler = _handlers.FirstOrDefault(item => item.CanHandle(command));
            if (handler is null)
            {
                throw new ArgumentException("Can't find handler");
            }
            return handler;
        }
    }
}
