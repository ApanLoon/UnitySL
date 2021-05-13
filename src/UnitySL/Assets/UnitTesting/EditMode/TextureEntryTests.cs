using System;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Primitives;
using NUnit.Framework;

namespace Tests
{
    public class TextureEntryTests
    {
        [Test]
        public void AvatarAppearanceExample()
        {
            byte[] buffer =
            {
                0x68, 0x00,
                0xc2, 0x28, 0xd1, 0xcf, 0x4b, 0x5d, 0x4b, 0xa8, 0x84, 0xf4, 0x89, 0x9a, 0x07, 0x96, 0xaa, 0x97, // image_id (Default)
                0x9f, 0xff, 0xff, 0x9f, 0xe1, 0x7f,                                                             // mask     (Exception 0)
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // image_id (Exception 0)
                0xc0, 0x9e, 0x00,                                                                               // mask     (Exception 1)
                0x3a, 0x36, 0x7d, 0x1c, 0xbe, 0xf1, 0x6d, 0x43, 0x75, 0x95, 0xe8, 0x8c, 0x1e, 0x3a, 0xad, 0xb3, // image_id (Exception 1)
                0x00,                                                                                           // end
                0x00, 0x00, 0x00, 0x00, // color (Default)
                0x00,                   // end
                0x00, 0x00, 0x80, 0x3f, // scale_s (Default)
                0x00,                   // end
                0x00, 0x00, 0x80, 0x3f, // scale_t (Default)
                0x00,                   // end
                0x00, 0x00,             // offset_s (Default)
                0x00,                   // end
                0x00, 0x00,             // offset_t (Default)
                0x00,                   // end
                0x00,                   // image_rot (Default)
                0x00,                   // end
                0x00,                   // bump (Default)
                0x00,                   // end
                0x00,                   // media_flags (Default)
                0x00,                   // end
                0x00,                   // glow (Default)
                0x00,                   // end
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // material_id (Default)
                0x00                                                                                            // end
            };

            int offset = 0;
            Guid a = BinarySerializer.DeSerializeGuid(new byte[] { 0xc2, 0x28, 0xd1, 0xcf, 0x4b, 0x5d, 0x4b, 0xa8, 0x84, 0xf4, 0x89, 0x9a, 0x07, 0x96, 0xaa, 0x97 }, ref offset, 16);
            offset = 0;
            Guid b = BinarySerializer.DeSerializeGuid(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, ref offset, 16);
            offset = 0;
            Guid c = BinarySerializer.DeSerializeGuid(new byte[] { 0x3a, 0x36, 0x7d, 0x1c, 0xbe, 0xf1, 0x6d, 0x43, 0x75, 0x95, 0xe8, 0x8c, 0x1e, 0x3a, 0xad, 0xb3 }, ref offset, 16);
            offset = 0;
            TextureEntry entry = BinarySerializer.DeSerializeTextureEntry(buffer, ref offset, buffer.Length);

            //for (int i = 0; i < entry.FaceTextures.Length; i++)
            //{
            //    TextureEntryFace face = entry.FaceTextures[i];
            //    UnityEngine.Debug.Log($"{i:x2}: {face.TextureId}");
            //}

            //var s = string.Concat(entry.FaceTextures.Select(x =>
            //{
            //    if (x.TextureId == a) return 'a';
            //    if (x.TextureId == b) return 'b';
            //    if (x.TextureId == c) return 'c';
            //    return ' ';
            //}));
            //UnityEngine.Debug.Log(s);

            Assert.AreEqual(b, entry.FaceTextures[0x00].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x01].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x02].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x03].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x04].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x05].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x06].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x07].TextureId);
            Assert.AreEqual(c, entry.FaceTextures[0x08].TextureId);
            Assert.AreEqual(c, entry.FaceTextures[0x09].TextureId);
            Assert.AreEqual(c, entry.FaceTextures[0x0a].TextureId);
            Assert.AreEqual(c, entry.FaceTextures[0x0b].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x0c].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x0d].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x0e].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x0f].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x10].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x11].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x12].TextureId);
            Assert.AreEqual(a, entry.FaceTextures[0x13].TextureId);
            Assert.AreEqual(c, entry.FaceTextures[0x14].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x15].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x16].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x17].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x18].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x19].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x1a].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x1b].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x1c].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x1d].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x1e].TextureId);
            Assert.AreEqual(b, entry.FaceTextures[0x1f].TextureId);
        }
    }
}
