using Yags.Log;

namespace Yags.Socket
{
    public class PolicySocketServer : SocketServerBase
    {
        public PolicySocketServer(LoggerFactoryFunc loggerFactory, ISocketHandler handler)
            : base(loggerFactory, handler)
        {

        }

        protected override int GetHeaderSize()
        {
            //"<policy-file-request/>".Length + 1
            return 23;
        }
    }
}