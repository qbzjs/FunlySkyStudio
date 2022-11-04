using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    public interface IFeatureBlender
    {
        // Perform the blending operation using the current state which holds the from/to profiles.
        void Blend(ProfileBlendingState state, BlendingHelper helper);
    }
}
