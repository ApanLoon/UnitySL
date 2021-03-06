﻿using Assets.Scripts.Extensions.SystemExtensions;
using NUnit.Framework;

namespace Tests
{
    public class SystemExtensionsTests
    {
        [Test]
        public void HexDump()
        {
            byte[] data =
            {
                0x00, 0x1f, 0x33, 0xbd, 0x8b, 0x77, 0x74, 0xd0, 0x2b, 0x98, 0xba, 0x51, 0x08, 0x00, 0x45, 0x00,
                0x00, 0x5b, 0x2f, 0x65, 0x00, 0x00, 0x80, 0x11, 0x00, 0x00, 0xc0, 0xa8, 0x00, 0x64, 0x34, 0x26,
                0x80, 0x63, 0xfe, 0x4b, 0x32, 0xe5, 0x00, 0x47, 0x75, 0xee, 0x00, 0x00, 0x00, 0x10, 0xab, 0x00,
                0xff, 0xff, 0xff, 0xfb, 0x0d, 0x45, 0xad, 0x00, 0x00, 0x46, 0xad, 0x00, 0x00, 0x48, 0xad, 0x00,
                0x00, 0x49, 0xad, 0x00, 0x00, 0x4b, 0xad, 0x00, 0x00, 0x4c, 0xad, 0x00, 0x00, 0x4d, 0xad, 0x00,
                0x00, 0x4e, 0xad, 0x00, 0x00, 0x4f, 0xad, 0x00, 0x00, 0x52, 0xad, 0x00, 0x00, 0x53, 0xad, 0x00,
                0x00, 0x57, 0xad, 0x00, 0x00, 0x58, 0xad, 0x00, 0x00
            };

            string expected =
                  "0000   00 1f 33 bd 8b 77 74 d0  2b 98 ba 51 08 00 45 00   ..3..wt.+..Q..E.\n"
                + "0010   00 5b 2f 65 00 00 80 11  00 00 c0 a8 00 64 34 26   .[/e.........d4&\n"
                + "0020   80 63 fe 4b 32 e5 00 47  75 ee 00 00 00 10 ab 00   .c.K2..Gu.......\n"
                + "0030   ff ff ff fb 0d 45 ad 00  00 46 ad 00 00 48 ad 00   .....E...F...H..\n"
                + "0040   00 49 ad 00 00 4b ad 00  00 4c ad 00 00 4d ad 00   .I...K...L...M..\n"
                + "0050   00 4e ad 00 00 4f ad 00  00 52 ad 00 00 53 ad 00   .N...O...R...S..\n"
                + "0060   00 57 ad 00 00 58 ad 00  00                        .W...X...\n";
            string actual = data.ToHexDump();
            Assert.AreEqual (expected, actual);
        }

    }
}
