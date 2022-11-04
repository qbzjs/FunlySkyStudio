using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Funly.SkyStudio
{
    public static class ColorBlendingExtensions
    {
        // Copy the color with a clear alpha.
        public static Color Clear(this Color color)
        {
            return new Color(color.r, color.g, color.b, 0);
        }
    }
}