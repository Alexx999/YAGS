using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Yags.Socket
{
    public static class SocketAsyncExtension
    {
        public static Task<System.Net.Sockets.Socket> AcceptAsync(this System.Net.Sockets.Socket socket)
        {
            var endAccept = new Func<IAsyncResult, System.Net.Sockets.Socket>(socket.EndAccept);
            return Task<System.Net.Sockets.Socket>.Factory.FromAsync(socket.BeginAccept, endAccept, null);
        }

        public static Task DisconnectAsync(this System.Net.Sockets.Socket socket, bool reuseSocket)
        {
            return Task.Factory.FromAsync(socket.BeginDisconnect, socket.EndDisconnect, reuseSocket, null);
        }

        public static Task<AcceptResult> AcceptAsync(this System.Net.Sockets.Socket socket, System.Net.Sockets.Socket acceptSocket, int receiveSize)
        {
            var tcs = new TaskCompletionSource<AcceptResult>(socket);
            socket.BeginAccept(acceptSocket, receiveSize, ar =>
            {
                var t = (TaskCompletionSource<AcceptResult>)ar.AsyncState;
                var s = (System.Net.Sockets.Socket)t.Task.AsyncState;
                try
                {
                    byte[] buff;
                    int bytesRead;
                    var resultSocket = s.EndAccept(out buff, out bytesRead, ar);
                    t.TrySetResult(new AcceptResult(resultSocket, buff, bytesRead));
                }
                catch (Exception exc)
                {
                    t.TrySetException(exc);
                }
            }, tcs);
            return tcs.Task;
        }

        public static Task<AcceptResult> AcceptAsync(this System.Net.Sockets.Socket socket, int receiveSize)
        {
            return AcceptAsync(socket, null, receiveSize); 
        }

        public static Task<int> ReceiveAsync(this System.Net.Sockets.Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginReceive(buffer, offset, size, socketFlags, ar =>
            {
                var t = (TaskCompletionSource<int>)ar.AsyncState;
                var s = (System.Net.Sockets.Socket)t.Task.AsyncState;
                try
                {
                    t.TrySetResult(s.EndReceive(ar));
                }
                catch (Exception exc)
                {
                    t.TrySetException(exc);
                }
            }, tcs);
            return tcs.Task;
        }

        public static Task<int> SendAsync(this System.Net.Sockets.Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            var tcs = new TaskCompletionSource<int>(socket);
            socket.BeginSend(buffer, offset, size, socketFlags, ar =>
            {
                var t = (TaskCompletionSource<int>)ar.AsyncState;
                var s = (System.Net.Sockets.Socket)t.Task.AsyncState;
                try
                {
                    t.TrySetResult(s.EndSend(ar));
                }
                catch (Exception exc)
                {
                    t.TrySetException(exc);
                }
            }, tcs);
            return tcs.Task;
        }
    }

    public struct AcceptResult
    {
        public readonly System.Net.Sockets.Socket Socket;
        public readonly byte[] Buffer;
        public readonly int BytesTransferred;

        public AcceptResult(System.Net.Sockets.Socket socket, byte[] buffer, int bytesTransferred)
        {
            Socket = socket;
            Buffer = buffer;
            BytesTransferred = bytesTransferred;
        }
    }
}
