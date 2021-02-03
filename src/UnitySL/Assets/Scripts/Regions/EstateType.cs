
// estate constants. Need to match first few entries in indra.estate table.
public enum EstateType
{
    All = 0, // will not match in db, reserved key for logic
    Mainland = 1,
    Orientation = 2,
    Internal = 3,
    Showcase = 4,
    Teen = 5,
    LastLinden = 5 // last linden owned/managed estate
}
