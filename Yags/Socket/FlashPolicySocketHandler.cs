using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Yags.Log;

namespace Yags.Socket
{
    public class FlashPolicySocketHandler : SocketHandlerBase
    {
        private byte[] _socketPolicyFile;
        private const string PolicyRequestString = "<policy-file-request/>\0";

        public FlashPolicySocketHandler(LoggerFactoryFunc loggerFactory, byte[] socketPolicyFile) : base(loggerFactory)
        {
            _socketPolicyFile = socketPolicyFile;
            _logger = LogHelper.CreateLogger(loggerFactory, GetType());
        }

        public override async Task Handle(System.Net.Sockets.Socket socket, AcceptResult result)
        {
            var str = Encoding.UTF8.GetString(result.Buffer, 0, result.BytesTransferred);
            if (str.Equals(PolicyRequestString))
            {
                await SendResponse(socket, _socketPolicyFile);
            }

            socket.Shutdown(SocketShutdown.Both);
        }
    }
}