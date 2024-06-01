using AG.DnsServer.UDP;
using System.Net.Sockets;

namespace AG.DnsServer.Extensions
{
    public static class SocketExtensions
    {
        public static SocketAwaitable ReceiveFromAsync(this Socket socket, SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.ReceiveFromAsync(awaitable.m_eventArgs))
            {
                awaitable.m_wasCompleted = true;
            }
            return awaitable;
        }

        public static SocketAwaitable SendToAsync(this Socket socket, SocketAwaitable awaitable)
        {
            awaitable.Reset();
            if (!socket.SendToAsync(awaitable.m_eventArgs))
            {
                awaitable.m_wasCompleted = true;
            }
            return awaitable;
        }
    }
}
