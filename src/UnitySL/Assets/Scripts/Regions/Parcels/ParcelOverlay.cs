using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Region;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Regions.Parcels
{
    /// <summary>
    /// Keeps track of parcel overlay on the terrain as well as the mini map.
    /// </summary>
    public class ParcelOverlay
    {
        public Texture2D ParcelOverlayMinimapBorderTexture { get; protected set; }

        protected Region Region { get; set; }
        protected int ParcelGridsPerEdge;
        protected OwnershipFlags[] Data;
        protected bool IsDirty = false;

        public ParcelOverlay(Region region, float width)
        {
            Region = region;
            ParcelGridsPerEdge = (int)(width / Parcel.PARCEL_GRID_STEP_METERS);
            Data = new OwnershipFlags[ParcelGridsPerEdge * ParcelGridsPerEdge];

            ParcelOverlayMinimapBorderTexture = new Texture2D(ParcelGridsPerEdge, ParcelGridsPerEdge, TextureFormat.RGBA32, false); // TODO: The minimap texture should be higher resolution to make the parcel lines more crisp.
            ParcelOverlayMinimapBorderTexture.wrapMode = TextureWrapMode.Clamp;
        }

        public void UpdateData(ParcelOverlayMessage message)
        {
            int size = ParcelGridsPerEdge * ParcelGridsPerEdge;
            int chunkSize = size / Parcel.PARCEL_OVERLAY_CHUNKS;
            Array.Copy(message.Data, 0, Data, message.SequenceId * chunkSize, chunkSize);

            UpdateMinimapTextures();
        }

        protected void UpdateMinimapTextures()
        {
            Color[] pixels = ParcelOverlayMinimapBorderTexture.GetPixels();

            for (int y = 0; y < ParcelGridsPerEdge; y++)
            {
                for (int x = 0; x < ParcelGridsPerEdge; x++)
                {
                    int offset = y * ParcelGridsPerEdge + x;
                    pixels[offset] = ColourByOwnership(Data[offset]);
                }
            }
            ParcelOverlayMinimapBorderTexture.SetPixels(pixels);
            ParcelOverlayMinimapBorderTexture.Apply();

        }

        protected Color ColourByOwnership(OwnershipFlags ownership)
        {
            if ((ownership & (OwnershipFlags.WestLine | OwnershipFlags.SouthLine)) != 0)
            {
                return Color.white;
            }

            switch (ownership & OwnershipFlags.ColourMask)
            {
                case OwnershipFlags.Public:
                    return new Color(1f, 1f, 1f, 0f); // Transparent
                case OwnershipFlags.Owned:
                    return new Color(1f, 0f, 0f, 0.4f);
                case OwnershipFlags.Group:
                    return new Color(0f, 0.72f, 0.72f, 0.4f);

                case OwnershipFlags.Self:
                    return new Color(0f, 1f, 0f, 0.4f);
                case OwnershipFlags.ForSale:
                    return new Color(1f, 0.5f, 0f, 0.4f);
                case OwnershipFlags.Auction:
                    return new Color(0.5f, 0f, 1f, 0.4f);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
