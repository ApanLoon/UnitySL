using System;

namespace Assets.Scripts.Character
{
    [Flags]
    public enum CharacterSex
    {
        SEX_FEMALE = 0x01,
        SEX_MALE = 0x02,
        SEX_BOTH = 0x03  // values chosen to allow use as a bit field.
    }

    public class VisualParameter
    {
    }
}
