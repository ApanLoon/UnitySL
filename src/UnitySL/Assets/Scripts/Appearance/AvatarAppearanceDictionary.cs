using Assets.Scripts.Common;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Appearance
{
    /// <summary>
    /// Holds dictionary static entries for textures, baked textures, meshes, etc.; i.e.
    /// information that is common to all avatars.
    /// 
    /// This holds const data - it is initialised once and the contents never change after that.
    /// </summary>
    public class AvatarAppearanceDictionary
    {
        public static readonly AvatarAppearanceDictionary Instance = new AvatarAppearanceDictionary();

        public enum TextureIndex
        {
            Invalid = -1,
            HeadBodyPaint = 0,
            UpperShirt,
            LowerPants,
            EyesIris,
            Hair,
            UpperBodyPaint,
            LowerBodyPaint,
            LowerShoes,
            HeadBaked, // Pre-composited
            UpperBaked, // Pre-composited
            LowerBaked, // Pre-composited
            EyesBaked, // Pre-composited
            LowerSocks,
            UpperJacket,
            LowerJacket, // Pre-composited
            UpperGloves,
            UpperUnderShirt,
            LowerUnderPants,
            Skirt,
            SkirtBaked, // Pre-composited
            HairBaked, // Pre-composited
            LowerAlpha,
            UpperAlpha,
            HeadAlpha,
            EyesAlpha,
            HairAlpha,
            HeadTattoo,
            UpperTattoo,
            LowerTattoo,
            HeadUniversalTattoo,
            UpperUniversalTattoo,
            LowerUniversalTattoo,
            SkirtTattoo,
            HairTattoo,
            EyesTattoo,
            LeftArmTattoo,
            LeftLegTattoo,
            Aux1Tattoo,
            Aux2Tattoo,
            Aux3Tattoo,
            LeftArmBaked, // Pre-composited
            LeftLegBaked, // Pre-composited
            Aux1Baked, // Pre-composited
            Aux2Baked, // Pre-composited
            Aux3Baked, // Pre-composited
            NumIndices
        }

        public enum BakedTextureIndex
        {
            Head = 0,
            Upper,
            Lower,
            Eyes,
            Skirt,
            Hair,
            LeftArm,
            LeftLeg,
            Aux1,
            Aux2,
            Aux3,
            NumIndices
        }

        /// <summary>
        /// Reference IDs for each mesh. Used as indices for vector of joints
        /// </summary>
        public enum MeshIndex
        {
            Hair = 0,
            Head,
            EyeLash,
            UpperBody,
            LowerBody,
            EyeBallLeft,
            EyeBallRight,
            Skirt,
            NumIndices
        }

        #region Initialisation
        protected AvatarAppearanceDictionary()
        {
            CreateAssociations();
        }

        /// <summary>
        /// Baked textures are composites of textures; for each such composited texture,
        /// map it to the baked texture.
        /// </summary>
        protected void CreateAssociations()
        {
            foreach (KeyValuePair<BakedTextureIndex, BakedEntry> keyValuePair in BakedTextures)
            {
                BakedTextureIndex bakedIndex = keyValuePair.Key;
                BakedEntry bakedEntry = keyValuePair.Value;

                // For each texture that this baked texture index affects, associate those textures
                // with this baked texture index.
                foreach (TextureIndex textureIndex in bakedEntry.LocalTextures)
                {
                    Textures[textureIndex].IsUsedByBakedTexture = true;
                    Textures[textureIndex].BakedTextureIndex = bakedIndex;
                }
            }
        }
        #endregion Initialisation

        #region LocalAndBakedTextures
        public class TextureEntry
        {
            public TextureEntry(
                string name,
                bool isLocal,
                BakedTextureIndex bakedTextureIndex = BakedTextureIndex.NumIndices,
                string defaultImageName = "",
                WearableType wearableType = WearableType.Invalid)
            {
                IsLocal = isLocal;
                IsBaked = !isLocal;
                IsUsedByBakedTexture = bakedTextureIndex != BakedTextureIndex.NumIndices;
                BakedTextureIndex = bakedTextureIndex;
                DefaultImageName = defaultImageName;
                WearableType = wearableType;
            }

            public string DefaultImageName { get; set; }
            public WearableType WearableType { get; }
            public bool IsLocal { get; set; }
            public bool IsBaked { get; set; }
            public bool IsUsedByBakedTexture { get; set; }
            public BakedTextureIndex BakedTextureIndex { get; set; }
        }

        public Dictionary<TextureIndex, TextureEntry> Textures { get; } =
            new Dictionary<TextureIndex, TextureEntry>()
            {
                {
                    TextureIndex.HeadBodyPaint,
                    new TextureEntry("head_bodypaint", true, BakedTextureIndex.NumIndices, "", WearableType.Skin)
                },
                {
                    TextureIndex.UpperShirt,
                    new TextureEntry("upper_shirt", true, BakedTextureIndex.NumIndices, "UIImgDefaultShirtUUID",
                        WearableType.Shirt)
                },
                {
                    TextureIndex.LowerPants,
                    new TextureEntry("lower_pants", true, BakedTextureIndex.NumIndices, "UIImgDefaultPantsUUID",
                        WearableType.Pants)
                },
                {
                    TextureIndex.EyesIris,
                    new TextureEntry("eyes_iris", true, BakedTextureIndex.NumIndices, "UIImgDefaultEyesUUID",
                        WearableType.Eyes)
                },
                {
                    TextureIndex.Hair,
                    new TextureEntry("hair_grain", true, BakedTextureIndex.NumIndices, "UIImgDefaultHairUUID",
                        WearableType.Hair)
                },
                {
                    TextureIndex.UpperBodyPaint,
                    new TextureEntry("upper_bodypaint", true, BakedTextureIndex.NumIndices, "", WearableType.Skin)
                },
                {
                    TextureIndex.LowerBodyPaint,
                    new TextureEntry("lower_bodypaint", true, BakedTextureIndex.NumIndices, "", WearableType.Skin)
                },
                {
                    TextureIndex.LowerShoes,
                    new TextureEntry("lower_shoes", true, BakedTextureIndex.NumIndices, "UIImgDefaultShoesUUID",
                        WearableType.Shoes)
                },
                {
                    TextureIndex.LowerSocks,
                    new TextureEntry("lower_socks", true, BakedTextureIndex.NumIndices, "UIImgDefaultSocksUUID",
                        WearableType.Socks)
                },
                {
                    TextureIndex.UpperJacket,
                    new TextureEntry("upper_jacket", true, BakedTextureIndex.NumIndices, "UIImgDefaultJacketUUID",
                        WearableType.Jacket)
                },
                {
                    TextureIndex.LowerJacket,
                    new TextureEntry("lower_jacket", true, BakedTextureIndex.NumIndices, "UIImgDefaultJacketUUID",
                        WearableType.Jacket)
                },
                {
                    TextureIndex.UpperGloves,
                    new TextureEntry("upper_gloves", true, BakedTextureIndex.NumIndices, "UIImgDefaultGlovesUUID",
                        WearableType.Gloves)
                },
                {
                    TextureIndex.UpperUnderShirt,
                    new TextureEntry("upper_undershirt", true, BakedTextureIndex.NumIndices,
                        "UIImgDefaultUnderwearUUID", WearableType.UnderShirt)
                },
                {
                    TextureIndex.LowerUnderPants,
                    new TextureEntry("lower_underpants", true, BakedTextureIndex.NumIndices,
                        "UIImgDefaultUnderwearUUID", WearableType.UnderPants)
                },
                {
                    TextureIndex.Skirt,
                    new TextureEntry("skirt", true, BakedTextureIndex.NumIndices, "UIImgDefaultSkirtUUID",
                        WearableType.Skirt)
                },

                {
                    TextureIndex.LowerAlpha,
                    new TextureEntry("lower_Alpha", true, BakedTextureIndex.NumIndices, "UIImgDefaultAlphaUUID",
                        WearableType.Alpha)
                },
                {
                    TextureIndex.UpperAlpha,
                    new TextureEntry("upper_Alpha", true, BakedTextureIndex.NumIndices, "UIImgDefaultAlphaUUID",
                        WearableType.Alpha)
                },
                {
                    TextureIndex.HeadAlpha,
                    new TextureEntry("head_Alpha", true, BakedTextureIndex.NumIndices, "UIImgDefaultAlphaUUID",
                        WearableType.Alpha)
                },
                {
                    TextureIndex.EyesAlpha,
                    new TextureEntry("eyes_Alpha", true, BakedTextureIndex.NumIndices, "UIImgDefaultAlphaUUID",
                        WearableType.Alpha)
                },
                {
                    TextureIndex.HairAlpha,
                    new TextureEntry("hair_Alpha", true, BakedTextureIndex.NumIndices, "UIImgDefaultAlphaUUID",
                        WearableType.Alpha)
                },

                {
                    TextureIndex.HeadTattoo,
                    new TextureEntry("head_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Tattoo)
                },
                {
                    TextureIndex.UpperTattoo,
                    new TextureEntry("upper_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Tattoo)
                },
                {
                    TextureIndex.LowerTattoo,
                    new TextureEntry("lower_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Tattoo)
                },

                {
                    TextureIndex.HeadUniversalTattoo,
                    new TextureEntry("head_Universal_tattoo", true, BakedTextureIndex.NumIndices, "",
                        WearableType.Universal)
                },
                {
                    TextureIndex.UpperUniversalTattoo,
                    new TextureEntry("upper_Universal_tattoo", true, BakedTextureIndex.NumIndices, "",
                        WearableType.Universal)
                },
                {
                    TextureIndex.LowerUniversalTattoo,
                    new TextureEntry("lower_Universal_tattoo", true, BakedTextureIndex.NumIndices, "",
                        WearableType.Universal)
                },
                {
                    TextureIndex.SkirtTattoo,
                    new TextureEntry("skirt_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },
                {
                    TextureIndex.HairTattoo,
                    new TextureEntry("hair_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },
                {
                    TextureIndex.EyesTattoo,
                    new TextureEntry("eyes_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },
                {
                    TextureIndex.LeftArmTattoo,
                    new TextureEntry("leftarm_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },
                {
                    TextureIndex.LeftLegTattoo,
                    new TextureEntry("leftleg_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },
                {
                    TextureIndex.Aux1Tattoo,
                    new TextureEntry("aux1_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },
                {
                    TextureIndex.Aux2Tattoo,
                    new TextureEntry("aux2_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },
                {
                    TextureIndex.Aux3Tattoo,
                    new TextureEntry("aux3_tattoo", true, BakedTextureIndex.NumIndices, "", WearableType.Universal)
                },

                {TextureIndex.HeadBaked, new TextureEntry("head-baked", false, BakedTextureIndex.Head, "head")},
                {TextureIndex.UpperBaked, new TextureEntry("upper-baked", false, BakedTextureIndex.Upper, "upper")},
                {TextureIndex.LowerBaked, new TextureEntry("lower-baked", false, BakedTextureIndex.Lower, "lower")},
                {TextureIndex.EyesBaked, new TextureEntry("eyes-baked", false, BakedTextureIndex.Eyes, "eyes")},
                {TextureIndex.HairBaked, new TextureEntry("hair-baked", false, BakedTextureIndex.Hair, "hair")},
                {TextureIndex.SkirtBaked, new TextureEntry("skirt-baked", false, BakedTextureIndex.Skirt, "skirt")},
                {
                    TextureIndex.LeftArmBaked,
                    new TextureEntry("leftarm-baked", false, BakedTextureIndex.LeftArm, "leftarm")
                },
                {
                    TextureIndex.LeftLegBaked,
                    new TextureEntry("leftleg-baked", false, BakedTextureIndex.LeftLeg, "leftleg")
                },
                {TextureIndex.Aux1Baked, new TextureEntry("aux1-baked", false, BakedTextureIndex.Aux1, "aux1")},
                {TextureIndex.Aux2Baked, new TextureEntry("aux2-baked", false, BakedTextureIndex.Aux2, "aux2")},
                {TextureIndex.Aux3Baked, new TextureEntry("aux3-baked", false, BakedTextureIndex.Aux3, "aux3")}
            };

        protected Dictionary<TextureIndex, Guid> GuidByTextureIndex = new Dictionary<TextureIndex,Guid>()
        {
            { TextureIndex.HeadBaked,    IndraConstants.IMG_USE_BAKED_HEAD    },
            { TextureIndex.UpperBaked,   IndraConstants.IMG_USE_BAKED_UPPER   },
            { TextureIndex.LowerBaked,   IndraConstants.IMG_USE_BAKED_LOWER   },
            { TextureIndex.EyesBaked,    IndraConstants.IMG_USE_BAKED_EYES    },
            { TextureIndex.SkirtBaked,   IndraConstants.IMG_USE_BAKED_SKIRT   },
            { TextureIndex.HairBaked,    IndraConstants.IMG_USE_BAKED_HAIR    },
            { TextureIndex.LeftArmBaked, IndraConstants.IMG_USE_BAKED_LEFTARM },
            { TextureIndex.LeftLegBaked, IndraConstants.IMG_USE_BAKED_LEFTLEG },
            { TextureIndex.Aux1Baked,    IndraConstants.IMG_USE_BAKED_AUX1    },
            { TextureIndex.Aux2Baked,    IndraConstants.IMG_USE_BAKED_AUX2    },
            { TextureIndex.Aux3Baked,    IndraConstants.IMG_USE_BAKED_AUX3    }
        };
        #endregion LocalAndBakedTextures

        #region Meshes
        public class MeshEntry
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="bakedTextureIndex"></param>
            /// <param name="name">Name of mesh type as they are used in avatar_lad.xml</param>
            /// <param name="lod"></param>
            public MeshEntry(BakedTextureIndex bakedTextureIndex, string name, byte lod)
            {
                BakedTextureIndex = bakedTextureIndex;
                Name = name;
                Lod = lod;
            }

            public string Name { get; set; }
            /// <summary>
            /// Level of Detail for this mesh.  Must match levels of detail present in avatar_lad.xml
            /// Otherwise meshes will be unable to be found, or levels of detail will be ignored
            /// </summary>
            public byte Lod { get; }

            public BakedTextureIndex BakedTextureIndex { get; }
        }

        public Dictionary<MeshIndex, MeshEntry> MeshEntries { get; } = new Dictionary<MeshIndex, MeshEntry>()
        {
            { MeshIndex.Hair,         new MeshEntry(BakedTextureIndex.Hair,  "hairMesh",         6) },
            { MeshIndex.Head,         new MeshEntry(BakedTextureIndex.Head,  "headMesh",         5) },
            { MeshIndex.EyeLash,      new MeshEntry(BakedTextureIndex.Head,  "eyelashMesh",      1) }, // no baked mesh associated currently
            { MeshIndex.UpperBody,    new MeshEntry(BakedTextureIndex.Upper, "upperBodyMesh",    5) },
            { MeshIndex.LowerBody,    new MeshEntry(BakedTextureIndex.Lower, "lowerBodyMesh",    5) },
            { MeshIndex.EyeBallLeft,  new MeshEntry(BakedTextureIndex.Eyes,  "eyeBallLeftMesh",  2) },
            { MeshIndex.EyeBallRight, new MeshEntry(BakedTextureIndex.Eyes,  "eyeBallRightMesh", 2) },
            { MeshIndex.Skirt,        new MeshEntry(BakedTextureIndex.Skirt, "skirtMesh",        5) }
        };
        #endregion Meshes

        #region BakedTextures
        public class BakedEntry
        {
            public BakedEntry(
                TextureIndex textureIndex,
                string name,
                string hashName)
            {
                Name = name;
                //WearableHashId = Guid.Parse(hashName);
                TextureIndex = textureIndex;
            }

            public string Name { get; set; }
            public TextureIndex TextureIndex { get; set; }
            public List<TextureIndex> LocalTextures { get; set; } = new List<TextureIndex>();

            public Guid WearableHashId { get; set; }
            public List<WearableType> Wearables { get; set; } = new List<WearableType>();
        }

        public Dictionary<BakedTextureIndex, BakedEntry> BakedTextures { get; } =
            new Dictionary<BakedTextureIndex, BakedEntry>
            {
                {
                    BakedTextureIndex.Head,
                    new BakedEntry(TextureIndex.HeadBaked, "head", "a4b9dc38-e13b-4df9-b284-751efb0566ff")
                    {
                        LocalTextures =
                        {
                            TextureIndex.HeadBodyPaint, TextureIndex.HeadTattoo, TextureIndex.HeadAlpha,
                            TextureIndex.HeadUniversalTattoo
                        },
                        Wearables =
                        {
                            WearableType.Shape, WearableType.Skin, WearableType.Hair, WearableType.Tattoo, WearableType.Alpha, WearableType.Universal
                        }
                    }
                },

                {
                    BakedTextureIndex.Upper,
                    new BakedEntry(TextureIndex.UpperBaked, "upper_body", "5943ff64-d26c-4a90-a8c0-d61f56bd98d4")
                    {
                        LocalTextures =
                        {
                            TextureIndex.UpperShirt, TextureIndex.UpperBodyPaint, TextureIndex.UpperJacket,
                            TextureIndex.UpperGloves, TextureIndex.UpperUnderShirt, TextureIndex.UpperTattoo,
                            TextureIndex.UpperAlpha, TextureIndex.UpperUniversalTattoo
                        },
                        Wearables =
                        {
                            WearableType.Shape, WearableType.Skin, WearableType.Shirt,
                            WearableType.Jacket, WearableType.Gloves, WearableType.UnderShirt,
                            WearableType.Tattoo, WearableType.Alpha, WearableType.Universal
                        }
                    }
                },

                {
                    BakedTextureIndex.Lower,
                    new BakedEntry(TextureIndex.LowerBaked, "lower_body", "2944ee70-90a7-425d-a5fb-d749c782ed7d")
                    {
                        LocalTextures =
                        {
                            TextureIndex.LowerPants, TextureIndex.LowerBodyPaint, TextureIndex.LowerShoes,
                            TextureIndex.LowerSocks, TextureIndex.LowerJacket, TextureIndex.LowerUnderPants,
                            TextureIndex.LowerTattoo, TextureIndex.LowerAlpha, TextureIndex.LowerUniversalTattoo
                        },
                        Wearables =
                        {
                            WearableType.Shape, WearableType.Skin, WearableType.Pants,
                            WearableType.Shoes, WearableType.Socks, WearableType.Jacket,
                            WearableType.UnderPants, WearableType.Tattoo, WearableType.Alpha,
                            WearableType.Universal
                        }
                    }
                },

                {
                    BakedTextureIndex.Eyes,
                    new BakedEntry(TextureIndex.EyesBaked, "eyes", "27b1bc0f-979f-4b13-95fe-b981c2ba9788")
                    {
                        LocalTextures = {TextureIndex.EyesIris, TextureIndex.EyesTattoo, TextureIndex.EyesAlpha},
                        Wearables = {WearableType.Eyes, WearableType.Universal, WearableType.Alpha}
                    }
                },

                {
                    BakedTextureIndex.Skirt,
                    new BakedEntry(TextureIndex.SkirtBaked, "skirt", "03e7e8cb-1368-483b-b6f3-74850838ba63")
                    {
                        LocalTextures = {TextureIndex.Skirt, TextureIndex.SkirtTattoo},
                        Wearables = {WearableType.Skirt, WearableType.Universal}
                    }
                },

                {
                    BakedTextureIndex.Hair,
                    new BakedEntry(TextureIndex.HairBaked, "hair", "a60e85a9-74e8-48d8-8a2d-8129f28d9b61")
                    {
                        LocalTextures = {TextureIndex.Hair, TextureIndex.HairTattoo, TextureIndex.HairAlpha},
                        Wearables = {WearableType.Hair, WearableType.Universal, WearableType.Alpha}
                    }
                },

                {
                    BakedTextureIndex.LeftArm,
                    new BakedEntry(TextureIndex.LeftArmBaked, "leftarm", "9f39febf-22d7-0087-79d1-e9e8c6c9ed19")
                    {
                        LocalTextures = {TextureIndex.LeftArmTattoo},
                        Wearables = {WearableType.Universal}
                    }
                },

                {
                    BakedTextureIndex.LeftLeg,
                    new BakedEntry(TextureIndex.LeftLegBaked, "leftleg", "054a7a58-8ed5-6386-0add-3b636fb28b78")
                    {
                        LocalTextures = {TextureIndex.LeftLegTattoo},
                        Wearables = {WearableType.Universal}
                    }
                },

                {
                    BakedTextureIndex.Aux1,
                    new BakedEntry(TextureIndex.Aux1Baked, "aux1", "790c11be-b25c-c17e-b4d2-6a4ad786b752")
                    {
                        LocalTextures = {TextureIndex.Aux1Tattoo},
                        Wearables = {WearableType.Universal}
                    }
                },

                {
                    BakedTextureIndex.Aux2,
                    new BakedEntry(TextureIndex.Aux2Baked, "aux2", "d78c478f-48c7-5928-5864-8d99fb1f521e")
                    {
                        LocalTextures = {TextureIndex.Aux2Tattoo},
                        Wearables = {WearableType.Universal}
                    }
                },

                {
                    BakedTextureIndex.Aux3,
                    new BakedEntry(TextureIndex.Aux3Baked, "aux3", "6a95dd53-edd9-aac8-f6d3-27ed99f3c3eb")
                    {
                        LocalTextures = {TextureIndex.Aux3Tattoo},
                        Wearables = {WearableType.Universal}
                    }
                }
            };

        protected Dictionary<Guid, BakedTextureIndex> BakedTextureIndexByGuid = new Dictionary<Guid, BakedTextureIndex>()
        {
            { IndraConstants.IMG_USE_BAKED_EYES,    BakedTextureIndex.Eyes    },
            { IndraConstants.IMG_USE_BAKED_HAIR,    BakedTextureIndex.Hair    },
            { IndraConstants.IMG_USE_BAKED_HEAD,    BakedTextureIndex.Head    },
            { IndraConstants.IMG_USE_BAKED_LOWER,   BakedTextureIndex.Lower   },
            { IndraConstants.IMG_USE_BAKED_SKIRT,   BakedTextureIndex.Skirt   },
            { IndraConstants.IMG_USE_BAKED_UPPER,   BakedTextureIndex.Upper   },
            { IndraConstants.IMG_USE_BAKED_LEFTARM, BakedTextureIndex.LeftArm },
            { IndraConstants.IMG_USE_BAKED_LEFTLEG, BakedTextureIndex.LeftLeg },
            { IndraConstants.IMG_USE_BAKED_AUX1,    BakedTextureIndex.Aux1    },
            { IndraConstants.IMG_USE_BAKED_AUX2,    BakedTextureIndex.Aux2    },
            { IndraConstants.IMG_USE_BAKED_AUX3,    BakedTextureIndex.Aux3    }
        };
        #endregion BakedTextures

        #region ConvenienceFunctions

        /// <summary>
        /// Convert from baked texture to associated texture; e.g. BAKED_HEAD -> TEX_HEAD_BAKED
        /// </summary>
        /// <param name="index"></param>
        public static TextureIndex BakedToLocalTextureIndex(BakedTextureIndex index)
        {
            return Instance.BakedTextures[index].TextureIndex;
        }

        /// <summary>
        /// Find a baked texture index based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static BakedTextureIndex FindBakedByRegionName(string name)
        {
            for (int i = 0; i < (int)BakedTextureIndex.NumIndices; i++)
            {
                if (Instance.BakedTextures[(BakedTextureIndex)i].Name == name)
                {
                    return (BakedTextureIndex)i;
                }
            }
            return BakedTextureIndex.NumIndices;
        }

        public static BakedTextureIndex FindBakedByImageName(string name)
        {
            for (int i = 0; i < (int)BakedTextureIndex.NumIndices; i++)
            {
                BakedEntry be = Instance.BakedTextures[(BakedTextureIndex) i];
                TextureEntry te = Instance.Textures[be.TextureIndex];
                if (te.DefaultImageName == name)
                {
                    return (BakedTextureIndex)i;
                }
            }
            return BakedTextureIndex.NumIndices;
        }

        /// <summary>
        /// Given a texture entry, determine which wearable type owns it. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static WearableType GetWearableType(TextureIndex index)
        {
            return Instance.Textures[index].WearableType;
        }

        public static bool IsBakedImageId(Guid id)
        {
            return    (id == IndraConstants.IMG_USE_BAKED_EYES)
                   || (id == IndraConstants.IMG_USE_BAKED_HAIR)
                   || (id == IndraConstants.IMG_USE_BAKED_HEAD)
                   || (id == IndraConstants.IMG_USE_BAKED_LOWER)
                   || (id == IndraConstants.IMG_USE_BAKED_SKIRT)
                   || (id == IndraConstants.IMG_USE_BAKED_UPPER)
                   || (id == IndraConstants.IMG_USE_BAKED_LEFTARM)
                   || (id == IndraConstants.IMG_USE_BAKED_LEFTLEG)
                   || (id == IndraConstants.IMG_USE_BAKED_AUX1)
                   || (id == IndraConstants.IMG_USE_BAKED_AUX2)
                   || (id == IndraConstants.IMG_USE_BAKED_AUX3);
        }

        public static BakedTextureIndex AssetIdToBakedTextureIndex(Guid id)
        {
            return Instance.BakedTextureIndexByGuid.ContainsKey(id) ? Instance.BakedTextureIndexByGuid[id] : BakedTextureIndex.NumIndices;
        }

        public static Guid LocalTextureIndexToMagicId(TextureIndex index)
        {
            return Instance.GuidByTextureIndex.ContainsKey(index) ? Instance.GuidByTextureIndex[index] : Guid.Empty;
        }
        #endregion ConvenienceFunctions
    }
}
