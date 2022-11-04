using UnityEngine;

namespace HLODSystem.Extensions
{
    public static class BoundsExtension
    {
        public static bool IsPartOf(this Bounds bounds, ref Bounds target)
        {
            return (double) bounds.min.x >= (double) target.min.x &&
                   (double) bounds.max.x <= (double) target.max.x &&
                   (double) bounds.min.y >= (double) target.min.y &&
                   (double) bounds.max.y <= (double) target.max.y &&
                   (double) bounds.min.z >= (double) target.min.z &&
                   (double) bounds.max.z <= (double) target.max.z;
        }
        
        public static Bounds TransformBounds(this Bounds bounds, Matrix4x4 matrix)
        {
            var xa = matrix.GetColumn(0) * bounds.min.x;
            var xb = matrix.GetColumn(0) * bounds.max.x;
 
            var ya = matrix.GetColumn(1) * bounds.min.y;
            var yb = matrix.GetColumn(1) * bounds.max.y;
 
            var za = matrix.GetColumn(2) * bounds.min.z;
            var zb = matrix.GetColumn(2) * bounds.max.z;
 
            var col4Pos = matrix.GetColumn(3);
 
            var min = new Vector3
            {
                x = Mathf.Min(xa.x, xb.x) + Mathf.Min(ya.x, yb.x) + Mathf.Min(za.x, zb.x) + col4Pos.x,
                y = Mathf.Min(xa.y, xb.y) + Mathf.Min(ya.y, yb.y) + Mathf.Min(za.y, zb.y) + col4Pos.y,
                z = Mathf.Min(xa.z, xb.z) + Mathf.Min(ya.z, yb.z) + Mathf.Min(za.z, zb.z) + col4Pos.z
            };

            var max = new Vector3
            {
                x = Mathf.Max(xa.x, xb.x) + Mathf.Max(ya.x, yb.x) + Mathf.Max(za.x, zb.x) + col4Pos.x,
                y = Mathf.Max(xa.y, xb.y) + Mathf.Max(ya.y, yb.y) + Mathf.Max(za.y, zb.y) + col4Pos.y,
                z = Mathf.Max(xa.z, xb.z) + Mathf.Max(ya.z, yb.z) + Mathf.Max(za.z, zb.z) + col4Pos.z
            };
            bounds.SetMinMax(min, max);
            return bounds;
        }

    }
}

