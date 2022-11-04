using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    public enum ProfileFeatureBlendingMode
    {
        // Do nothing, this feature isn't enabled in "from" or "to" profile.
        None,

        // Fade feature "from" value in profile, "to" other value in profile that also supports it.
        Normal,

        // Fade out a feature during 0-.5, since it doesn't exist in destination profile.
        FadeFeatureOut,

        // Fade in a feature during 0.5-1 since it only exists in the destination profile.
        FadeFeatureIn
    }
}