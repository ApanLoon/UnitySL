
using UnityEngine;

namespace Assets.Scripts.MathExtensions
{
    public static class QuaternionExtensions
    {
        public static bool IsFinite(this Quaternion q)
        {
            return    !float.IsInfinity(q.x)
                   && !float.IsInfinity(q.y)
                   && !float.IsInfinity(q.z)
                   && !float.IsInfinity(q.w);
        }
    }
}
