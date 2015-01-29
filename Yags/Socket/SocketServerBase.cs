using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Yags.Log;

namespace Yags.Socket
{
    public abstract class SocketServerBase : Core.Server
    {
        private ISocketHandler _handler;
        private System.Net.Sockets.Socket _serverSocket;
        private bool _isListening;
        private FieldInfo _rightEndPointField;

        protected SocketServerBase(LoggerFactoryFunc loggerFactory, ISocketHandler handler)
            : base(loggerFactory)
        {
            _handler = handler;
            _startNextRequestAsync = new Action(ProcessRequestsAsync);
            _startNextRequestError = new Action<Task>(StartNextRequestError);
            SetRequestProcessingLimits(DefaultMaxAccepts, DefaultMaxRequests);

            _rightEndPointField = typeof(System.Net.Sockets.Socket).GetField("m_RightEndPoint", BindingFlags.Instance | BindingFlags.NonPublic);
            PacketHeader.Size = Marshal.SizeOf(typeof(PacketHeader));
        }

        private void StartNextRequestError(Task faultedTask)
        {
        }

        private async void ProcessRequestsAsync()
        {
            System.Net.Sockets.Socket socket = null;
            while (IsListening() && CanAcceptMoreRequests())
            {
                var headerSize = GetHeaderSize();
                AcceptResult result = await _serverSocket.AcceptAsync(socket, headerSize);
#if DEBUG
                LogHelper.LogVerbose(_logger,
                    socket == result.Socket
                        ? "Socket client connected, socket reused"
                        : "Socket client connected, socket created");
#endif
                socket = result.Socket;
                StartListening();
                await _handler.Handle(socket, result);
                await socket.DisconnectAsync(true);
                //hack which allows to reuse connection. Probably issue in .NET
                _rightEndPointField.SetValue(socket, null);

                LogHelper.LogVerbose(_logger, "Socket client disconnected");
            }
        }

        protected override bool CanAcceptMoreRequests()
        {
            return true;
        }

        protected override bool IsListening()
        {
            return _isListening;
        }

        public override void Start(int port)
        {
            var myEndpoint = new IPEndPoint(IPAddress.Any, port); 
            _serverSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(myEndpoint);
            _serverSocket.Listen(int.MaxValue);
            _isListening = true;
            StartListening();
        }

        public override void Dispose()
        {
            _serverSocket.Dispose();
            base.Dispose();
        }

        protected abstract int GetHeaderSize();
    }
}