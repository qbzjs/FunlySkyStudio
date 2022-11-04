using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    /// <summary>
    ///  Name of the latest version of the rendering support content packages.
    /// </summary>
    public abstract class RenderingContentPackages
    {
        public static int packageVersion = 24;
        public static string builtinPackage = $"Builtin-Content-v{packageVersion}.unitypackage";
        public static string urpPackage = $"URP-Content-v{packageVersion}.unitypackage";
    }
}
