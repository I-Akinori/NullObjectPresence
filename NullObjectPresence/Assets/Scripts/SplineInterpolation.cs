using System.Collections.Generic;
using UnityEngine;

namespace NullObjectPresence
{
    public static class SplineInterpolation
    {
        public static Vector3 Interpolation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var a = new Vector3[4];
            a[3] = p1;
            a[2] = (p2 - p0) * 0.5f;
            a[0] = (p3 - p1) * 0.5f - 2 * p2 + a[2] + 2 * a[3];
            a[1] = 3 * p2 - (p3 - p1) * 0.5f - 2 * a[2] - 3 * a[3];

            var f = Vector3.zero;
            for (var i = 0; i < 4; i++)
            {
                f += a[i] * Mathf.Pow(t, 3 - i);
            }

            return f;
        }
        
        public static Vector3 Interpolation(List<Vector3> points, float t)
        {
            var count = points.Count;
            if (count < 2)
            {
                return points[0];
            }
            var index = Mathf.FloorToInt(t * (count - 1));
            var t2 = t * (count - 1) - index;
            var p0 = points[index + (index > 0 ? -1 : 0)];
            var p1 = points[index];
            var p2 = points[index + 1];
            var p3 = points[index + (index + 2 < count ? 2 : 1)];
            
            return Interpolation(p0, p1, p2, p3, t2);
        }
    }
}