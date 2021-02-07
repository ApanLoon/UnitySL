using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SurfaceTests
    {
        [Test]
        public void InitPatchDecompressor_size_0x10()
        {
            Patch.InitPatchDecompressor(0x10);
            float[] dequantizeTable = // size=0x10
            {
                1f, 3f, 5f, 7f, 9f, 11f, 13f, 15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f,
                3f, 5f, 7f, 9f, 11f, 13f, 15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f,
                5f, 7f, 9f, 11f, 13f, 15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f,
                7f, 9f, 11f, 13f, 15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f,
                9f, 11f, 13f, 15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f,
                11f, 13f, 15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f,
                13f, 15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f,
                15f, 17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f,
                17f, 19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f,
                19f, 21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f, 49f,
                21f, 23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f, 49f, 51f,
                23f, 25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f, 49f, 51f, 53f,
                25f, 27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f, 49f, 51f, 53f, 55f,
                27f, 29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f, 49f, 51f, 53f, 55f, 57f,
                29f, 31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f, 49f, 51f, 53f, 55f, 57f, 59f,
                31f, 33f, 35f, 37f, 39f, 41f, 43f, 45f, 47f, 49f, 51f, 53f, 55f, 57f, 59f, 61f,
            };
            float[] patchICosines = // size=0x10
            {
                1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,
                0.99518472f, 0.956940353f, 0.881921232f, 0.773010433f, 0.634393275f, 0.471396655f, 0.290284634f, 0.0980171338f, -0.0980172232f, -0.290284723f, -0.471396834f, -0.634393275f, -0.773010492f, -0.881921351f, -0.956940353f, -0.99518472f,
                0.980785251f, 0.831469595f, 0.555570185f, 0.195090234f, -0.195090324f, -0.555570364f, -0.831469655f, -0.98078531f, -0.980785251f, -0.831469536f, -0.555570006f, -0.195090383f, 0.195090413f, 0.555570424f, 0.831469595f, 0.98078531f,
                0.956940353f, 0.634393275f, 0.0980171338f, -0.471396834f, -0.881921351f, -0.99518472f, -0.773010373f, -0.290284544f, 0.290285021f, 0.773010552f, 0.995184779f, 0.881921172f, 0.471396416f, -0.0980172753f, -0.634393573f, -0.956940353f,
                0.923879504f, 0.382683426f, -0.382683516f, -0.923879623f, -0.923879504f, -0.382683128f, 0.382683605f, 0.923879564f, 0.923879385f, 0.382683277f, -0.382683903f, -0.923879504f, -0.923879445f, -0.382682979f, 0.382683307f, 0.923879623f,
                0.881921232f, 0.0980171338f, -0.773010492f, -0.956940293f, -0.290284544f, 0.634393334f, 0.99518472f, 0.471396416f, -0.471396565f, -0.995184779f, -0.634392798f, 0.2902852f, 0.956940472f, 0.773010194f, -0.0980174541f, -0.881921351f,
                0.831469595f, -0.195090324f, -0.98078531f, -0.555570006f, 0.555570424f, 0.980785251f, 0.19509007f, -0.831469774f, -0.831469178f, 0.195090577f, 0.980785429f, 0.555570006f, -0.555570841f, -0.980785251f, -0.195089579f, 0.831469774f,
                0.773010433f, -0.471396834f, -0.956940293f, 0.0980174318f, 0.995184779f, 0.290284693f, -0.88192153f, -0.634392798f, 0.634393811f, 0.881920874f, -0.290285498f, -0.99518472f, -0.0980161056f, 0.956940651f, 0.471395671f, -0.773011267f,
                0.707106769f, -0.707106769f, -0.70710665f, 0.707107008f, 0.707106769f, -0.707107246f, -0.707106531f, 0.707106829f, 0.707106292f, -0.707107008f, -0.707106054f, 0.70710659f, 0.707106531f, -0.707107484f, -0.707106948f, 0.707107067f,
                0.634393275f, -0.881921351f, -0.290284544f, 0.995184779f, -0.0980172753f, -0.956940353f, 0.471396863f, 0.773010194f, -0.77301085f, -0.471396804f, 0.956940651f, 0.0980167314f, -0.995184779f, 0.290285528f, 0.881921172f, -0.634394348f,
                0.555570185f, -0.98078531f, 0.195090413f, 0.831469357f, -0.831469774f, -0.195090219f, 0.98078531f, -0.555570841f, -0.555570543f, 0.980785549f, -0.195091546f, -0.831468999f, 0.831470132f, 0.195089549f, -0.980785131f, 0.555570602f,
                0.471396655f, -0.99518472f, 0.634393334f, 0.290284693f, -0.956940353f, 0.773010671f, 0.0980164334f, -0.881921172f, 0.88192153f, -0.0980181023f, -0.773009598f, 0.956940293f, -0.290284932f, -0.634392738f, 0.995184839f, -0.471396357f,
                0.382683426f, -0.923879504f, 0.923879564f, -0.382683903f, -0.382682979f, 0.923879325f, -0.923879743f, 0.382683903f, 0.382682055f, -0.923879325f, 0.9238801f, -0.382683933f, -0.382682055f, 0.923879325f, -0.9238801f, 0.382683963f,
                0.290284634f, -0.773010373f, 0.99518472f, -0.88192153f, 0.471396863f, 0.0980164334f, -0.634392321f, 0.956940055f, -0.956940472f, 0.634393334f, -0.0980187505f, -0.471395642f, 0.881920874f, -0.995184958f, 0.773011684f, -0.290286183f,
                0.195090234f, -0.555570006f, 0.831469357f, -0.980785191f, 0.980785429f, -0.831469595f, 0.555571139f, -0.195091546f, -0.195088938f, 0.555568933f, -0.831468642f, 0.980785251f, -0.980785668f, 0.831470847f, -0.555572212f, 0.195092842f,
                0.0980171338f, -0.290284544f, 0.471396416f, -0.634392798f, 0.773010194f, -0.881921172f, 0.956940055f, -0.995184779f, 0.995184779f, -0.956940651f, 0.881921232f, -0.77301091f, 0.634394348f, -0.471398622f, 0.290287405f, -0.0980169028f,
            };
            int[] deCopyMatrix = new int[0x100];
            deCopyMatrix[0x0] = 0x0; deCopyMatrix[0x1] = 0x1; deCopyMatrix[0x10] = 0x2; deCopyMatrix[0x20] = 0x3; deCopyMatrix[0x11] = 0x4; deCopyMatrix[0x2] = 0x5; deCopyMatrix[0x3] = 0x6; deCopyMatrix[0x12] = 0x7; deCopyMatrix[0x21] = 0x8; deCopyMatrix[0x30] = 0x9; deCopyMatrix[0x40] = 0xa; deCopyMatrix[0x31] = 0xb; deCopyMatrix[0x22] = 0xc; deCopyMatrix[0x13] = 0xd; deCopyMatrix[0x4] = 0xe; deCopyMatrix[0x5] = 0xf; deCopyMatrix[0x14] = 0x10; deCopyMatrix[0x23] = 0x11; deCopyMatrix[0x32] = 0x12; deCopyMatrix[0x41] = 0x13; deCopyMatrix[0x50] = 0x14; deCopyMatrix[0x60] = 0x15; deCopyMatrix[0x51] = 0x16; deCopyMatrix[0x42] = 0x17; deCopyMatrix[0x33] = 0x18; deCopyMatrix[0x24] = 0x19; deCopyMatrix[0x15] = 0x1a; deCopyMatrix[0x6] = 0x1b; deCopyMatrix[0x7] = 0x1c; deCopyMatrix[0x16] = 0x1d; deCopyMatrix[0x25] = 0x1e; deCopyMatrix[0x34] = 0x1f; deCopyMatrix[0x43] = 0x20; deCopyMatrix[0x52] = 0x21; deCopyMatrix[0x61] = 0x22; deCopyMatrix[0x70] = 0x23; deCopyMatrix[0x80] = 0x24; deCopyMatrix[0x71] = 0x25; deCopyMatrix[0x62] = 0x26; deCopyMatrix[0x53] = 0x27; deCopyMatrix[0x44] = 0x28; deCopyMatrix[0x35] = 0x29; deCopyMatrix[0x26] = 0x2a; deCopyMatrix[0x17] = 0x2b; deCopyMatrix[0x8] = 0x2c; deCopyMatrix[0x9] = 0x2d; deCopyMatrix[0x18] = 0x2e; deCopyMatrix[0x27] = 0x2f; deCopyMatrix[0x36] = 0x30; deCopyMatrix[0x45] = 0x31; deCopyMatrix[0x54] = 0x32; deCopyMatrix[0x63] = 0x33; deCopyMatrix[0x72] = 0x34; deCopyMatrix[0x81] = 0x35; deCopyMatrix[0x90] = 0x36; deCopyMatrix[0xa0] = 0x37; deCopyMatrix[0x91] = 0x38; deCopyMatrix[0x82] = 0x39; deCopyMatrix[0x73] = 0x3a; deCopyMatrix[0x64] = 0x3b; deCopyMatrix[0x55] = 0x3c; deCopyMatrix[0x46] = 0x3d; deCopyMatrix[0x37] = 0x3e; deCopyMatrix[0x28] = 0x3f; deCopyMatrix[0x19] = 0x40; deCopyMatrix[0xa] = 0x41; deCopyMatrix[0xb] = 0x42; deCopyMatrix[0x1a] = 0x43; deCopyMatrix[0x29] = 0x44; deCopyMatrix[0x38] = 0x45; deCopyMatrix[0x47] = 0x46; deCopyMatrix[0x56] = 0x47; deCopyMatrix[0x65] = 0x48; deCopyMatrix[0x74] = 0x49; deCopyMatrix[0x83] = 0x4a; deCopyMatrix[0x92] = 0x4b; deCopyMatrix[0xa1] = 0x4c; deCopyMatrix[0xb0] = 0x4d; deCopyMatrix[0xc0] = 0x4e; deCopyMatrix[0xb1] = 0x4f; deCopyMatrix[0xa2] = 0x50; deCopyMatrix[0x93] = 0x51; deCopyMatrix[0x84] = 0x52; deCopyMatrix[0x75] = 0x53; deCopyMatrix[0x66] = 0x54; deCopyMatrix[0x57] = 0x55; deCopyMatrix[0x48] = 0x56; deCopyMatrix[0x39] = 0x57; deCopyMatrix[0x2a] = 0x58; deCopyMatrix[0x1b] = 0x59; deCopyMatrix[0xc] = 0x5a; deCopyMatrix[0xd] = 0x5b; deCopyMatrix[0x1c] = 0x5c; deCopyMatrix[0x2b] = 0x5d; deCopyMatrix[0x3a] = 0x5e; deCopyMatrix[0x49] = 0x5f; deCopyMatrix[0x58] = 0x60; deCopyMatrix[0x67] = 0x61; deCopyMatrix[0x76] = 0x62; deCopyMatrix[0x85] = 0x63; deCopyMatrix[0x94] = 0x64; deCopyMatrix[0xa3] = 0x65; deCopyMatrix[0xb2] = 0x66; deCopyMatrix[0xc1] = 0x67; deCopyMatrix[0xd0] = 0x68; deCopyMatrix[0xe0] = 0x69; deCopyMatrix[0xd1] = 0x6a; deCopyMatrix[0xc2] = 0x6b; deCopyMatrix[0xb3] = 0x6c; deCopyMatrix[0xa4] = 0x6d; deCopyMatrix[0x95] = 0x6e; deCopyMatrix[0x86] = 0x6f; deCopyMatrix[0x77] = 0x70; deCopyMatrix[0x68] = 0x71; deCopyMatrix[0x59] = 0x72; deCopyMatrix[0x4a] = 0x73; deCopyMatrix[0x3b] = 0x74; deCopyMatrix[0x2c] = 0x75; deCopyMatrix[0x1d] = 0x76; deCopyMatrix[0xe] = 0x77; deCopyMatrix[0xf] = 0x78; deCopyMatrix[0x1e] = 0x79; deCopyMatrix[0x2d] = 0x7a; deCopyMatrix[0x3c] = 0x7b; deCopyMatrix[0x4b] = 0x7c; deCopyMatrix[0x5a] = 0x7d; deCopyMatrix[0x69] = 0x7e; deCopyMatrix[0x78] = 0x7f; deCopyMatrix[0x87] = 0x80; deCopyMatrix[0x96] = 0x81; deCopyMatrix[0xa5] = 0x82; deCopyMatrix[0xb4] = 0x83; deCopyMatrix[0xc3] = 0x84; deCopyMatrix[0xd2] = 0x85; deCopyMatrix[0xe1] = 0x86; deCopyMatrix[0xf0] = 0x87; deCopyMatrix[0xf1] = 0x88; deCopyMatrix[0xe2] = 0x89; deCopyMatrix[0xd3] = 0x8a; deCopyMatrix[0xc4] = 0x8b; deCopyMatrix[0xb5] = 0x8c; deCopyMatrix[0xa6] = 0x8d; deCopyMatrix[0x97] = 0x8e; deCopyMatrix[0x88] = 0x8f; deCopyMatrix[0x79] = 0x90; deCopyMatrix[0x6a] = 0x91; deCopyMatrix[0x5b] = 0x92; deCopyMatrix[0x4c] = 0x93; deCopyMatrix[0x3d] = 0x94; deCopyMatrix[0x2e] = 0x95; deCopyMatrix[0x1f] = 0x96; deCopyMatrix[0x2f] = 0x97; deCopyMatrix[0x3e] = 0x98; deCopyMatrix[0x4d] = 0x99; deCopyMatrix[0x5c] = 0x9a; deCopyMatrix[0x6b] = 0x9b; deCopyMatrix[0x7a] = 0x9c; deCopyMatrix[0x89] = 0x9d; deCopyMatrix[0x98] = 0x9e; deCopyMatrix[0xa7] = 0x9f; deCopyMatrix[0xb6] = 0xa0; deCopyMatrix[0xc5] = 0xa1; deCopyMatrix[0xd4] = 0xa2; deCopyMatrix[0xe3] = 0xa3; deCopyMatrix[0xf2] = 0xa4; deCopyMatrix[0xf3] = 0xa5; deCopyMatrix[0xe4] = 0xa6; deCopyMatrix[0xd5] = 0xa7; deCopyMatrix[0xc6] = 0xa8; deCopyMatrix[0xb7] = 0xa9; deCopyMatrix[0xa8] = 0xaa; deCopyMatrix[0x99] = 0xab; deCopyMatrix[0x8a] = 0xac; deCopyMatrix[0x7b] = 0xad; deCopyMatrix[0x6c] = 0xae; deCopyMatrix[0x5d] = 0xaf; deCopyMatrix[0x4e] = 0xb0; deCopyMatrix[0x3f] = 0xb1; deCopyMatrix[0x4f] = 0xb2; deCopyMatrix[0x5e] = 0xb3; deCopyMatrix[0x6d] = 0xb4; deCopyMatrix[0x7c] = 0xb5; deCopyMatrix[0x8b] = 0xb6; deCopyMatrix[0x9a] = 0xb7; deCopyMatrix[0xa9] = 0xb8; deCopyMatrix[0xb8] = 0xb9; deCopyMatrix[0xc7] = 0xba; deCopyMatrix[0xd6] = 0xbb; deCopyMatrix[0xe5] = 0xbc; deCopyMatrix[0xf4] = 0xbd; deCopyMatrix[0xf5] = 0xbe; deCopyMatrix[0xe6] = 0xbf; deCopyMatrix[0xd7] = 0xc0; deCopyMatrix[0xc8] = 0xc1; deCopyMatrix[0xb9] = 0xc2; deCopyMatrix[0xaa] = 0xc3; deCopyMatrix[0x9b] = 0xc4; deCopyMatrix[0x8c] = 0xc5; deCopyMatrix[0x7d] = 0xc6; deCopyMatrix[0x6e] = 0xc7; deCopyMatrix[0x5f] = 0xc8; deCopyMatrix[0x6f] = 0xc9; deCopyMatrix[0x7e] = 0xca; deCopyMatrix[0x8d] = 0xcb; deCopyMatrix[0x9c] = 0xcc; deCopyMatrix[0xab] = 0xcd; deCopyMatrix[0xba] = 0xce; deCopyMatrix[0xc9] = 0xcf; deCopyMatrix[0xd8] = 0xd0; deCopyMatrix[0xe7] = 0xd1; deCopyMatrix[0xf6] = 0xd2; deCopyMatrix[0xf7] = 0xd3; deCopyMatrix[0xe8] = 0xd4; deCopyMatrix[0xd9] = 0xd5; deCopyMatrix[0xca] = 0xd6; deCopyMatrix[0xbb] = 0xd7; deCopyMatrix[0xac] = 0xd8; deCopyMatrix[0x9d] = 0xd9; deCopyMatrix[0x8e] = 0xda; deCopyMatrix[0x7f] = 0xdb; deCopyMatrix[0x8f] = 0xdc; deCopyMatrix[0x9e] = 0xdd; deCopyMatrix[0xad] = 0xde; deCopyMatrix[0xbc] = 0xdf; deCopyMatrix[0xcb] = 0xe0; deCopyMatrix[0xda] = 0xe1; deCopyMatrix[0xe9] = 0xe2; deCopyMatrix[0xf8] = 0xe3; deCopyMatrix[0xf9] = 0xe4; deCopyMatrix[0xea] = 0xe5; deCopyMatrix[0xdb] = 0xe6; deCopyMatrix[0xcc] = 0xe7; deCopyMatrix[0xbd] = 0xe8; deCopyMatrix[0xae] = 0xe9; deCopyMatrix[0x9f] = 0xea; deCopyMatrix[0xaf] = 0xeb; deCopyMatrix[0xbe] = 0xec; deCopyMatrix[0xcd] = 0xed; deCopyMatrix[0xdc] = 0xee; deCopyMatrix[0xeb] = 0xef; deCopyMatrix[0xfa] = 0xf0; deCopyMatrix[0xfb] = 0xf1; deCopyMatrix[0xec] = 0xf2; deCopyMatrix[0xdd] = 0xf3; deCopyMatrix[0xce] = 0xf4; deCopyMatrix[0xbf] = 0xf5; deCopyMatrix[0xcf] = 0xf6; deCopyMatrix[0xde] = 0xf7; deCopyMatrix[0xed] = 0xf8; deCopyMatrix[0xfc] = 0xf9; deCopyMatrix[0xfd] = 0xfa; deCopyMatrix[0xee] = 0xfb; deCopyMatrix[0xdf] = 0xfc; deCopyMatrix[0xef] = 0xfd; deCopyMatrix[0xfe] = 0xfe; deCopyMatrix[0xff] = 0xff;

            for (int i = 0; i < dequantizeTable.Length; i++)
            {
                Assert.AreEqual(dequantizeTable[i], Patch._PatchDequantizeTable[i], $"DequantizeTable: Incorrect value at position {i}.");
            }

            for (int i = 0; i < patchICosines.Length; i++)
            {
                Assert.AreEqual(patchICosines[i], Patch._PatchICosines[i], $"PatchICosines: Incorrect value at position {i}.");
            }

            for (int i = 0; i < deCopyMatrix.Length; i++)
            {
                Assert.AreEqual(deCopyMatrix[i], Patch._DeCopyMatrix[i], $"DeCopyMatrix: Incorrect value at position {i}.");
            }
        }

    }
}
