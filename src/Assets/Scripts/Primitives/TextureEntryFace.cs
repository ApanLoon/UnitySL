
using System;
using UnityEngine;

/// <summary>
/// A single textured face. Don't instantiate this class yourself, use the
/// methods in TextureEntry
/// </summary>
public class TextureEntryFace
{
    public Color Colour { get; set; }
    public float RepeatU { get; set; }
    public float RepeatV { get; set; }
    public float OffsetU { get; set; }
    public float OffsetV { get; set; }
    public float Rotation { get; set; }
    public float Glow { get; set; }
    public Bumpiness Bumpiness { get; set; }
    public Shininess Shininess { get; set; }
    public bool FullBright { get; set; }
    public bool HasMedia { get; set; }
    public MappingType TextureMappingType { get; set; }
    public Guid TextureId { get; set; }
    public Guid MaterialId { get; set; }

    // +----------+ S = Shiny
    // | SSFBBBBB | F = Fullbright
    // | 76543210 | B = Bumpmap
    // +----------+
    private const byte BUMP_MASK       = 0x1F;
    private const byte FULLBRIGHT_MASK = 0x20;
    private const byte SHINY_MASK      = 0xC0;
    // +----------+ M = Media Flags (web page)
    // | .....TTM | T = Texture Mapping
    // | 76543210 | . = Unused
    // +----------+
    private const byte MEDIA_MASK      = 0x01;
    private const byte TEX_MAP_MASK    = 0x06;


}

/// <summary>
/// The type of bump-mapping applied to a face
/// </summary>
public enum Bumpiness : byte
{
    None       = 0,
    Brightness = 1,
    Darkness   = 2,
    Woodgrain  = 3,
    Bark       = 4,
    Bricks     = 5,
    Checker    = 6,
    Concrete   = 7,
    Crustytile = 8,
    Cutstone   = 9,
    Discs      = 10,
    Gravel     = 11,
    Petridish  = 12,
    Siding     = 13,
    Stonetile  = 14,
    Stucco     = 15,
    Suction    = 16,
    Weave      = 17
}

/// <summary>
/// The level of shininess applied to a face
/// </summary>
public enum Shininess : byte
{
    None   = 0,
    Low    = 0x40,
    Medium = 0x80,
    High   = 0xC0
}

/// <summary>
/// The texture mapping style used for a face
/// </summary>
public enum MappingType : byte
{
    Default     = 0,
    Planar      = 2,
    Spherical   = 4,
    Cylindrical = 6
}
