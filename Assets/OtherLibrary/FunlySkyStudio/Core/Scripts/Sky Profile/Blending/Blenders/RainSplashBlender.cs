using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    /// <summary>
    /// Blender used for rain ground splashes during a sky profile transition.
    /// </summary>
    public class RainSplashBlender : FeatureBlender
    {
        protected override string featureKey => ProfileFeatureKeys.RainSplashFeature;
    
        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumber(ProfilePropertyKeys.RainSplashMaxConcurrentKey);
            helper.BlendNumber(ProfilePropertyKeys.RainSplashAreaStartKey);
            helper.BlendNumber(ProfilePropertyKeys.RainSplashAreaLengthKey);
            helper.BlendNumber(ProfilePropertyKeys.RainSplashScaleKey);
            helper.BlendNumber(ProfilePropertyKeys.RainSplashScaleVarienceKey);
            helper.BlendNumber(ProfilePropertyKeys.RainSplashIntensityKey);
            helper.BlendNumber(ProfilePropertyKeys.RainSplashSurfaceOffsetKey);
            helper.BlendColor(ProfilePropertyKeys.RainSplashTintColorKey);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(ProfilePropertyKeys.RainSplashIntensityKey);
            helper.BlendNumberIn(ProfilePropertyKeys.RainSplashMaxConcurrentKey);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(ProfilePropertyKeys.RainSplashIntensityKey);
            helper.BlendNumberOut(ProfilePropertyKeys.RainSplashMaxConcurrentKey);
        }
    }
}
