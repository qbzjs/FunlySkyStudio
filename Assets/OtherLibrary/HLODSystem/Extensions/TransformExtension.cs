using UnityEngine;

namespace HLODSystem.Extensions
{
    public static class TransformExtension
    {
        public static Bounds? GetBounds(this Transform transform)
        {
            var renders  = transform.GetComponentsInChildren<MeshRenderer>(true);
            if (renders.Length == 0)
            {
                return null;
            }
            var center = Vector3.zero;
            foreach (var child in renders){
                center += child.bounds.center;   
            }
            center /= renders.Length; 
            var bounds = new Bounds(center,Vector3.zero);
            foreach (var child in renders){
                bounds.Encapsulate(child.bounds);   
            }
            return bounds;
        }

        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isEditor)
                {
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
                }
                else
                {
                    Object.Destroy(transform.GetChild(i).gameObject);
                }
            }
        }

    }
}
