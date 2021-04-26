using System;

namespace Assets.Scripts.Regions.Parcels
{
    [Flags]
    public enum OwnershipFlags : byte
    {
        Public     = 0x00,
        Owned      = 0x01,
        Group      = 0x02,
        Self       = 0x03,
        ForSale    = 0x04,
        Auction    = 0x05,

        /// <summary>
        /// Bottom three bits are a colour index for the land overlay
        /// </summary>
        ColourMask = 0x07,

        /// <summary>
        /// Avatars not visible outside of parcel.  Used for 'see avs' feature, but must be off for compatibility
        /// </summary>
        HiddenNavs = 0x10,	

        SoundLocal = 0x20,

        /// <summary>
        /// Flag, property line on west edge
        /// </summary>
        WestLine = 0x40,

        /// <summary>
        /// Flag, property line on south edge
        /// </summary>
        SouthLine = 0x80	
    }

    public class Parcel
    {
        // Grid out of which parcels taken is stepped every 4 meters.
        public const float PARCEL_GRID_STEP_METERS = 4f;

        // Number of "chunks" in which parcel overlay data is sent
        // Chunk 0 = southern rows, entire width
        public const int PARCEL_OVERLAY_CHUNKS = 4;
    }
}
