
using System;

[Flags]
public enum RegionFlags : UInt64
{
    AllowDamage                        = (1 << 0), // Can you be hurt here? Should health be on?
    AllowLandmark                      = (1 << 1), // Can you make landmarks here?
    AllowSetHome                       = (1 << 2), 
    ResetHomeOnTeleport                = (1 << 3), // Do we reset the home position when someone teleports away from here?
    SunFixed                           = (1 << 4), // Does the sun move?
    AllowAccessOverride                = (1 << 5), // Does the estate owner allow private parcels?
    BlockTerraform                     = (1 << 6), // Can't change the terrain heightfield, even on owned parcels, but can plant trees and grass.
    BlockLandResell                    = (1 << 7), // Can't release, sell, or buy land.
    Sandbox                            = (1 << 8), // All content wiped once per night
    AllowEnvironmentOverride           = (1 << 9),
    SkipCollisions                     = (1 << 12), // Pin all non agent rigid bodies
    SkipScripts                        = (1 << 13),
    SkipPhysics                        = (1 << 14), // Skip all physics
    ExternallyVisible                  = (1 << 15),
    AllowReturnEncroachingObject       = (1 << 16),
    AllowReturnEncroachingEstateObject = (1 << 17),
    BlockDwell                         = (1 << 18),
    BlockFly                           = (1 << 19), // Is flight allowed?
    AllowDirectTeleport                = (1 << 20), // Is direct teleport (p2p) allowed?
    EstateSkipScripts                  = (1 << 21), // Is there an administrative override on scripts in the region at the moment. This is the similar skip scripts, except this flag is presisted in the database on an estate level.
    RestrictPushObject                 = (1 << 22),
    DenyAnonymous                      = (1 << 23),
    AllowParcelChanges                 = (1 << 26),
    BlockFlyOver                       = (1 << 27),
    AllowVoice                         = (1 << 28),
    BlockParcelSearch                  = (1 << 29),
    DenyAgeUnverify                    = (1 << 30),

    Default      = AllowLandmark | AllowSetHome | AllowParcelChanges | AllowVoice,
    PreludeSet   = ResetHomeOnTeleport,
    PreludeUnset = AllowLandmark | AllowSetHome,
    EstateMask   = ExternallyVisible | SunFixed | DenyAnonymous | DenyAgeUnverify
}

public static class RegionFlagsExtensions
{
    public static bool IsPrelude (this RegionFlags flags)
    {
        // definition of prelude does not depend on fixed-sun
        return    (flags & RegionFlags.PreludeUnset) == 0
               && (flags & RegionFlags.PreludeSet)   != 0;
    }

    public static RegionFlags SetPrelude(this RegionFlags flags)
    {
        return    (flags & ~RegionFlags.PreludeUnset)
                | (RegionFlags.PreludeSet | RegionFlags.SunFixed); // also set the sun-fixed flag
    }

    public static RegionFlags UnsetPrelude(this RegionFlags flags)
    {
        return (flags | RegionFlags.PreludeUnset) 
               & ~(RegionFlags.PreludeSet | RegionFlags.SunFixed);
    }
}


