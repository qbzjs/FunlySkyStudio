using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    /// <summary>
    /// Blender used for animating fog during a sky profile transition.
    /// </summary>
    public class FogBlender : FeatureBlender
    {
        protected override string featureKey => ProfileFeatureKeys.FogFeature;
    
        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumber(ProfilePropertyKeys.FogDensityKey);
            helper.BlendNumber(ProfilePropertyKeys.FogLengthKey);
            helper.BlendColor(ProfilePropertyKeys.FogColorKey);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(ProfilePropertyKeys.FogDensityKey);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(ProfilePropertyKeys.FogDensityKey);
        }
    }
}