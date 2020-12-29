using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Socks
{
    /// <summary>
    /// https://blog.zhaytam.com/2019/11/15/socks5-a-net-core-implementation-from-scratch/
    /// </summary>
    public static class Socks5
    {
        public static async Task<Socket> Connect(Func<Socket> socketFactory, Socks5Options options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var socket = socketFactory();
            await socket.ConnectAsync(options.ProxyHost, options.ProxyPort);
            await SelectAuth(socket, options);
            await Connect(socket, options);

            return socket;
        }

        private static async Task SelectAuth(Socket socket, Socks5Options options)
        {
            /*
            +----+----------+----------+
            | VER | NMETHODS | METHODS |
            +----+----------+----------+
            | 1  | 1        | 1 to 255 |
            +----+----------+----------+
             */
            var buffer = new byte[4] {
                5,
                2,
                Socks5Constants.AuthMethodNoAuthenticationRequired, Socks5Constants.AuthMethodUsernamePassword
            };
            await socket.SendAsync(buffer, SocketFlags.None);

            /*
            +-----+--------+
            | VER | METHOD |
            +-----+--------+
            | 1   | 1      |
            +-----+--------+
            */
            var response = new byte[2];
            var read = await socket.ReceiveAsync(response, SocketFlags.None);
            if (read != 2)
                throw new SocksocketException($"Failed to select an authentication method, the server sent {read} bytes.");

            if (response[1] == Socks5Constants.AuthMethodReplyNoAcceptableMethods)
            {
                socket.Close();
                throw new SocksocketException("The proxy destination does not accept the supported proxy client authentication methods.");
            }

            if (response[1] == Socks5Constants.AuthMethodUsernamePassword && options.Auth == AuthType.None)
            {
                socket.Close();
                throw new SocksocketException("The proxy destination requires a username and password for authentication.");
            }

            if (response[1] == Socks5Constants.AuthMethodNoAuthenticationRequired)
                return;

            await PerformAuth(socket, options);
        }

        private static async Task PerformAuth(Socket socket, Socks5Options options)
        {
            /*
            +-----+------+----------+------+----------+
            | VER | ULEN | UNAME    | PLEN | PASSWD   |
            +----+-------+----------+------+----------+
            | 1  | 1     | 1 to 255 | 1    | 1 to 255 |
            +----+-------+----------+------+----------+
            */
            var buffer = ConstructAuthBuffer(options.Credentials.Username, options.Credentials.Password);
            await socket.SendAsync(buffer, SocketFlags.None);

            /*
            +----+--------+
            |VER | STATUS |
            +----+--------+
            | 1  |   1    |
            +----+--------+
            */
            var response = new byte[2];
            var read = await socket.ReceiveAsync(response, SocketFlags.None);
            if (read != 2)
                throw new SocksocketException($"Failed to perform authentication, the server sent {read} bytes.");

            if (response[1] != 0)
            {
                socket.Close();
                throw new SocksocketException("Proxy authentication failed.");
            }
        }

        private static async Task Connect(Socket socket, Socks5Options options)
        {
            /*
            +-----+-----+-------+------+----------+----------+
            | VER | CMD | RSV   | ATYP | DST.ADDR | DST.PORT |
            +--- -+-----+-------+------+----------+----------+
            | 1   | 1   | X'00' | 1    | Variable | 2        |
            +-----+-----+-------+------+----------+----------+
            */

            var addressType = GetDestAddressType(options.DestinationHost);
            var destAddr = GetDestAddressBytes(addressType, options.DestinationHost);
            var destPort = GetDestPortBytes(options.DestinationPort);

            var buffer = new byte[6 + options.DestinationHost.Length];
            buffer[0] = 5;
            buffer[1] = Socks5Constants.CmdConnect;
            buffer[2] = Socks5Constants.Reserved;
            buffer[3] = addressType;
            destAddr.CopyTo(buffer, 4);
            destPort.CopyTo(buffer, 4 + destAddr.Length);

            await socket.SendAsync(buffer, SocketFlags.None);

            /*
            +---- +-----+-------+------+----------+----------+
            | VER | REP | RSV   | ATYP | BND.ADDR | BND.PORT |
            +-----+-----+-------+------+----------+----------+
            | 1   | 1   | X'00' | 1    | Variable | 2        |
            +-----+-----+-------+------+----------+----------+
            */

            var response = new byte[255];
            await socket.ReceiveAsync(response, SocketFlags.None);

            if (response[1] != Socks5Constants.CmdReplySucceeded)
                HandleProxyCommandError(response, options.DestinationHost, options.DestinationPort);
        }

        private static void HandleProxyCommandError(byte[] response, string destinationHost, int destinationPort)
        {
            var replyCode = response[1];
            string proxyErrorText;
            switch (replyCode)
            {
                case Socks5Constants.CmdReplyGeneralSocksServerFailure:
                    proxyErrorText = "a general socks destination failure occurred";
                    break;
                case Socks5Constants.CmdReplyConnectionNotAllowedByRuleset:
                    proxyErrorText = "the connection is not allowed by proxy destination rule set";
                    break;
                case Socks5Constants.CmdReplyNetworkUnreachable:
                    proxyErrorText = "the network was unreachable";
                    break;
                case Socks5Constants.CmdReplyHostUnreachable:
                    proxyErrorText = "the host was unreachable";
                    break;
                case Socks5Constants.CmdReplyConnectionRefused:
                    proxyErrorText = "the connection was refused by the remote network";
                    break;
                case Socks5Constants.CmdReplyTtlExpired:
                    proxyErrorText = "the time to live (TTL) has expired";
                    break;
                case Socks5Constants.CmdReplyCommandNotSupported:
                    proxyErrorText = "the command issued by the proxy client is not supported by the proxy destination";
                    break;
                case Socks5Constants.CmdReplyAddressTypeNotSupported:
                    proxyErrorText = "the address type specified is not supported";
                    break;
                default:
                    proxyErrorText = string.Format(CultureInfo.InvariantCulture,
                        "an unknown SOCKS reply with the code value '{0}' was received",
                        replyCode.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            string exceptionMsg = string.Format(CultureInfo.InvariantCulture,
                "proxy error: {0} for destination host {1} port number {2}.",
                proxyErrorText, destinationHost, destinationPort);

            throw new SocksocketException(exceptionMsg);
        }

        private static byte[] ConstructAuthBuffer(string username, string password)
        {
            var credentials = new byte[3 + username.Length + password.Length];

            credentials[0] = 0x01;
            credentials[1] = (byte)username.Length;
            Array.Copy(Encoding.ASCII.GetBytes(username), 0, credentials, 2, username.Length);
            credentials[username.Length + 2] = (byte)password.Length;
            Array.Copy(Encoding.ASCII.GetBytes(password), 0, credentials, 2, password.Length);

            return credentials;
        }

        private static byte GetDestAddressType(string host)
        {
            if (!IPAddress.TryParse(host, out var ipAddr))
                return Socks5Constants.AddrtypeDomainName;

            switch (ipAddr.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    return Socks5Constants.AddrtypeIpv4;
                case AddressFamily.InterNetworkV6:
                    return Socks5Constants.AddrtypeIpv6;
                default:
                    throw new SocksocketException(
                        string.Format("The host addess {0} of type '{1}' is not a supported address type.\n" +
                        "The supported types are InterNetwork and InterNetworkV6.", host,
                        Enum.GetName(typeof(AddressFamily), ipAddr.AddressFamily)));
            }

        }

        private static byte[] GetDestAddressBytes(byte addressType, string host)
        {
            switch (addressType)
            {
                case Socks5Constants.AddrtypeIpv4:
                case Socks5Constants.AddrtypeIpv6:
                    return IPAddress.Parse(host).GetAddressBytes();
                case Socks5Constants.AddrtypeDomainName:
                    byte[] bytes = new byte[host.Length + 1];
                    bytes[0] = Convert.ToByte(host.Length);
                    Encoding.ASCII.GetBytes(host).CopyTo(bytes, 1);
                    return bytes;
                default:
                    return null;
            }
        }

        private static byte[] GetDestPortBytes(int value)
        {
            return new byte[2]
            {
                Convert.ToByte(value / 256),
                Convert.ToByte(value % 256)
            };
        }

    }
}
