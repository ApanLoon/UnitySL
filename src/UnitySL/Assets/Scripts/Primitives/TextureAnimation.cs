
public enum TextureAnimationMode : byte
{
    /// <summary>Disable texture animation</summary>
    ANIM_OFF = 0x00,
    /// <summary>Enable texture animation</summary>
    ANIM_ON = 0x01,
    /// <summary>Loop when animating textures</summary>
    LOOP = 0x02,
    /// <summary>Animate in reverse direction</summary>
    REVERSE = 0x04,
    /// <summary>Animate forward then reverse</summary>
    PING_PONG = 0x08,
    /// <summary>Slide texture smoothly instead of frame-stepping</summary>
    SMOOTH = 0x10,
    /// <summary>Rotate texture instead of using frames</summary>
    ROTATE = 0x20,
    /// <summary>Scale texture instead of using frames</summary>
    SCALE = 0x40,
}
public class TextureAnimation
{
    public TextureAnimationMode Mode { get; set; }
    public uint Face { get; set; }
    public uint SizeX { get; set; }
    public uint SizeY { get; set; }
    public float Start { get; set; }
    public float Length { get; set; }
    public float Rate { get; set; }
}
