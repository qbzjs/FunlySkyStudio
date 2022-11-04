using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    // Blending used for rain downfall when transitioning sky profiles.
    public class RainBlender : FeatureBlender
    {
        protected override string featureKey => ProfileFeatureKeys.RainFeature;
    
        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumber(ProfilePropertyKeys.RainSoundVolumeKey);
            helper.BlendNumber(ProfilePropertyKeys.RainNearIntensityKey);
            helper.BlendNumber(ProfilePropertyKeys.RainNearSpeedKey);
            helper.BlendNumber(ProfilePropertyKeys.RainNearTextureTiling);
            helper.BlendNumber(ProfilePropertyKeys.RainFarIntensityKey);
            helper.BlendNumber(ProfilePropertyKeys.RainFarSpeedKey);
            helper.BlendNumber(ProfilePropertyKeys.RainFarTextureTiling);
            helper.BlendColor(ProfilePropertyKeys.RainTintColorKey);
            helper.BlendNumber(ProfilePropertyKeys.RainWindTurbulence);
            helper.BlendNumber(ProfilePropertyKeys.RainWindTurbulenceSpeed);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(ProfilePropertyKeys.RainSoundVolumeKey);
            helper.BlendNumberIn(ProfilePropertyKeys.RainNearIntensityKey);
            helper.BlendNumberIn(ProfilePropertyKeys.RainFarIntensityKey);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(ProfilePropertyKeys.RainSoundVolumeKey);
            helper.BlendNumberOut(ProfilePropertyKeys.RainNearIntensityKey);
            helper.BlendNumberOut(ProfilePropertyKeys.RainFarIntensityKey);
        }
    }
}
