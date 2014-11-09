using Yags.Log;

namespace Yags.Socket
{
    public class SocketServer : SocketServerBase
    {
        public SocketServer(LoggerFactoryFunc loggerFactory, ISocketHandler handler) : base(loggerFactory, handler)
        {

        }

        protected override int GetHeaderSize()
        {
            return PacketHeader.Size;
        }
    }
}
