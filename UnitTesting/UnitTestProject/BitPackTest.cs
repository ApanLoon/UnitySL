using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class BitPackTest
    {
        [TestMethod]
        public void GetBool()
        {
            byte[] buffer =
            {
                0x50
            };
            BitPack bitPack = new BitPack(buffer);

            Assert.AreEqual(false, bitPack.GetBool());
            Assert.AreEqual(true, bitPack.GetBool());
            Assert.AreEqual(false, bitPack.GetBool());
            Assert.AreEqual(true, bitPack.GetBool());
            Assert.AreEqual(false, bitPack.GetBool());
            Assert.AreEqual(false, bitPack.GetBool());
            Assert.AreEqual(false, bitPack.GetBool());
            Assert.AreEqual(false, bitPack.GetBool());

        }

        [TestMethod]
        public void GetUInt8()
        {
            byte[] buffer = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                buffer[i] = (byte)i;
            }
            BitPack bitPack = new BitPack(buffer);

            for (int i = 0; i < 256; i++)
            {
                byte b = bitPack.GetUInt8();
                Assert.AreEqual((byte)i, b);
            }
        }

        [TestMethod]
        public void GetUInt8_4()
        {
            byte[] buffer = new byte[8];
            byte v = 0;
            for (int i = 0; i < 8; i++)
            {
                buffer[i] = (byte)((byte)(v++ << 4) | (byte)(v++ & 0x0f));
            }
            BitPack bitPack = new BitPack(buffer);

            for (int i = 0; i < 16; i++)
            {
                v = bitPack.GetUInt8(4);
                Assert.AreEqual(i, v);
            }
        }

        [TestMethod]
        public void GetUInt8_8()
        {
            byte[] buffer = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                buffer[i] = (byte)i;
            }
            BitPack bitPack = new BitPack(buffer);

            for (int i = 0; i < 256; i++)
            {
                byte b = bitPack.GetUInt8(8);
                Assert.AreEqual((byte)i, b);
            }
        }

        [TestMethod]
        public void GetUInt16_Le()
        {
            byte[] buffer = new byte[65536 * 2];
            int destIndex = 0;
            for (int i = 0; i < 65536; i++)
            {
                buffer[destIndex++] = (byte)((i >> 0) & 0xff);
                buffer[destIndex++] = (byte)((i >> 8) & 0xff);
            }
            BitPack bitPack = new BitPack(buffer);

            for (int i = 0; i < 65536; i++)
            {
                UInt16 v = bitPack.GetUInt16_Le();
                Assert.AreEqual((UInt16)i, v);
            }
        }


        [TestMethod]
        public void GetUInt16_Be()
        {
            byte[] buffer = new byte[65536 * 2];
            int destIndex = 0;
            for (int i = 0; i < 65536; i++)
            {
                buffer[destIndex++] = (byte)((i >> 8) & 0xff);
                buffer[destIndex++] = (byte)((i >> 0) & 0xff);
            }
            BitPack bitPack = new BitPack(buffer);

            for (int i = 0; i < 65536; i++)
            {
                UInt16 v = bitPack.GetUInt16_Be();
                Assert.AreEqual((UInt16)i, v);
            }
        }

        [TestMethod]
        public void GetUInt16_Be_9()
        {
            UInt16[] values =
            {
                0x00ff,
                0x0100,
                0x0101
            };
            byte[] buffer =
            {
                0x7f, 0xc0, 0x20, 0x20
            };
            BitPack bitPack = new BitPack(buffer);

            for (int i = 0; i < values.Length; i++)
            {
                UInt16 v = bitPack.GetUInt16_Be(9);
                Assert.AreEqual((UInt16)values[i], v);
            }
        }

        [TestMethod]
        public void GetUInt32_Le()
        {
            UInt32[] values =
            {
                0x00000055,
                0x000000aa,

                0x000055aa,
                0x0000aa55,

                0x0055aa55,
                0x00aa55aa,

                0x55aa55aa,
                0xaa55aa55,

                0x01234567,
                0x89abcdef,

                0xfedcba98,
                0x76543210
            };
            byte[] buffer = new byte[values.Length * 4];
            int destIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                buffer[destIndex++] = (byte)((values[i] >>  0) & 0xff);
                buffer[destIndex++] = (byte)((values[i] >>  8) & 0xff);
                buffer[destIndex++] = (byte)((values[i] >> 16) & 0xff);
                buffer[destIndex++] = (byte)((values[i] >> 24) & 0xff);
            }
            BitPack bitPack = new BitPack(buffer);

            for (int i = 0; i < values.Length; i++)
            {
                UInt32 v = bitPack.GetUInt32_Le();
                Assert.AreEqual(values[i], v);
            }
        }
    }
}
