namespace Socks
{
    public class Socks5Constants
    {

        public const byte Reserved = 0x00;
        public const byte AuthNumberOfAuthMethodsSupported = 2;
        public const byte AuthMethodNoAuthenticationRequired = 0x00;
        public const byte AuthMethodGssapi = 0x01;
        public const byte AuthMethodUsernamePassword = 0x02;
        public const byte AuthMethodIanaAssignedRangeBegin = 0x03;
        public const byte AuthMethodIanaAssignedRangeEnd = 0x7f;
        public const byte AuthMethodReservedRangeBegin = 0x80;
        public const byte AuthMethodReservedRangeEnd = 0xfe;
        public const byte AuthMethodReplyNoAcceptableMethods = 0xff;
        public const byte CmdConnect = 0x01;
        public const byte CmdBind = 0x02;
        public const byte CmdUdpAssociate = 0x03;
        public const byte CmdReplySucceeded = 0x00;
        public const byte CmdReplyGeneralSocksServerFailure = 0x01;
        public const byte CmdReplyConnectionNotAllowedByRuleset = 0x02;
        public const byte CmdReplyNetworkUnreachable = 0x03;
        public const byte CmdReplyHostUnreachable = 0x04;
        public const byte CmdReplyConnectionRefused = 0x05;
        public const byte CmdReplyTtlExpired = 0x06;
        public const byte CmdReplyCommandNotSupported = 0x07;
        public const byte CmdReplyAddressTypeNotSupported = 0x08;
        public const byte AddrtypeIpv4 = 0x01;
        public const byte AddrtypeDomainName = 0x03;
        public const byte AddrtypeIpv6 = 0x04;

    }
}
