using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Socks
{
    public static class SocketExtensions
    {
        public static async Task<bool> SendAsync(this Socket socket, byte[] buffer, SocketFlags flags)
        {
            int n = await socket.SendAsync(new ArraySegment<byte>(buffer), flags);
            return n == buffer.Length;
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer, SocketFlags flags)
        {
            return await socket.ReceiveAsync(new ArraySegment<byte>(buffer), flags);
        }
    }
}
