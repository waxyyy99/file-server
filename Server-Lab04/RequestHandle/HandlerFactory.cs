using Server.RequestHandle.Commands.Interface;

namespace Server.RequestHandle
{
    public class HandlerFactory
    {
        private readonly IEnumerable<ICommandHandler> _handlers;
        public HandlerFactory(IEnumerable<ICommandHandler> handlers)
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
