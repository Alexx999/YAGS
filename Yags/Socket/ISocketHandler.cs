using System.Threading.Tasks;

namespace Yags.Socket
{
    public interface ISocketHandler
    {
        Task Handle(System.Net.Sockets.Socket socket, AcceptResult result);
    }
}