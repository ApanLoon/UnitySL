
using System;
using UnityEngine;

namespace Assets.Scripts.Extensions.MathExtensions
{
    public static class QuantizationExtensions
    {
        const UInt16 U16Max = 65535;
        public const float Oou16Max = 1f / U16Max;

        public static float ToFloat(this UInt16 integerValue, float lower, float upper)
        {
            float val = integerValue * Oou16Max;
            float delta = (upper - lower);
            val *= delta;
            val += lower;

            float maxError = delta * Oou16Max;

            // make sure that zero's come through as zero
            if (Mathf.Abs(val) < maxError)
                val = 0f;

            return val;
		}

        public static float ToFloat(this byte integerValue, float lower, float upper)
        {
            float val = integerValue * Oou16Max;
            float delta = (upper - lower);
            val *= delta;
            val += lower;

            float maxError = delta * Oou16Max;

            // make sure that zero's come through as zero
            if (Mathf.Abs(val) < maxError)
                val = 0f;

            return val;
        }

    }
}
