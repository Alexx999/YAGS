using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Yags.Core;
using Yags.Log;

namespace Yags.Socket
{
    public class SocketHandler : SocketHandlerBase
    {
        private CancellationToken _token = new CancellationToken();
        private MethodRunner _runner;

        public SocketHandler(LoggerFactoryFunc loggerFactory, MethodRunner runner)
            : base(loggerFactory)
        {
            _runner = runner;
        }

        public override async Task Handle(System.Net.Sockets.Socket socket, AcceptResult result)
        {
            var header = new PacketHeader();
            bool headerAccepted = false;
            if (result.BytesTransferred >= PacketHeader.Size)
            {
                header = DataUtils.BytesToStruct<PacketHeader>(result.Buffer);
                headerAccepted = true;
            }
            while (socket.Connected)
            {
                if (!headerAccepted)
                {
                    PacketHeader? headerResult = await GetHeader(socket);
                    if (!headerResult.HasValue) break;
                    header = headerResult.Value;
                }

                if (header.Version != 1)
                {
                    LogHelper.LogInfo(_logger, "Header version mismatch");
                    break;
                }

                var msg = await GetMessage(socket, header);
                if (msg == null)
                {
                    break;
                }

                var response = await _runner.Execute(msg, _token);

                var responseHeader = new PacketHeader {Version = 1, DataLength = response != null? (uint) response.Length: 0};

                var headerBytes = new byte[PacketHeader.Size];

                DataUtils.StructToBytes(responseHeader, headerBytes);

                if (!await SendResponse(socket, headerBytes))
                {
                    break;
                }

                if (responseHeader.DataLength != 0)
                {
                    if (!await SendResponse(socket, response))
                    {
                        break;
                    }
                }

                headerAccepted = false;
            }

            socket.Shutdown(SocketShutdown.Both);
        }

        private static async Task<PacketHeader?> GetHeader(System.Net.Sockets.Socket socket)
        {
            var buff = await GetData(socket, PacketHeader.Size);
            if(buff == null) return null;
            return DataUtils.BytesToStruct<PacketHeader>(buff);
        }
        
        public static Task<byte[]> GetMessage(System.Net.Sockets.Socket socket, PacketHeader header)
        {
            return GetData(socket, (int)header.DataLength);
        }

    }
}