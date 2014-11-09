using System.Net.Sockets;
using System.Threading.Tasks;
using Yags.Log;

namespace Yags.Socket
{
    public abstract class SocketHandlerBase : ISocketHandler
    {
        protected LoggerFunc _logger;

        protected SocketHandlerBase(LoggerFactoryFunc loggerFactory)
        {
            _logger = LogHelper.CreateLogger(loggerFactory, GetType());
        }

        public abstract Task Handle(System.Net.Sockets.Socket socket, AcceptResult result);

        protected static async Task<byte[]> GetData(System.Net.Sockets.Socket socket, int size)
        {
            var buff = new byte[size];
            int bytesRead = 0;
            while (bytesRead < size)
            {
                int readSize;
                try
                {
                    readSize = await socket.ReceiveAsync(buff, bytesRead, size - bytesRead, SocketFlags.None);
                }
                catch (SocketException)
                {
                    return null;
                }
                bytesRead += readSize;
                if (readSize == 0)
                {
                    return null;
                }
            }
            return buff;
        }


        protected static async Task<bool> SendResponse(System.Net.Sockets.Socket socket, byte[] response)
        {
            int bytesSent = 0;
            while (bytesSent < response.Length)
            {
                int sendSize;
                try
                {
                    sendSize = await socket.SendAsync(response, bytesSent, response.Length - bytesSent, SocketFlags.None);
                }
                catch (SocketException)
                {
                    return false;
                }
                bytesSent += sendSize;
                if (sendSize == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}