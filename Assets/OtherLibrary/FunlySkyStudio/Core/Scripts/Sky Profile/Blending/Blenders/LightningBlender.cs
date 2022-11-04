using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    /// <summary>
    /// Blender used for animating lightning during a profile transition.
    /// </summary>
    public class LightningBlender : FeatureBlender
    {
        protected override string featureKey => ProfileFeatureKeys.LightningFeature;
    
        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendColor(ProfilePropertyKeys.LightningTintColorKey);
            helper.BlendNumber(ProfilePropertyKeys.ThunderSoundVolumeKey);
            helper.BlendNumber(ProfilePropertyKeys.ThunderSoundDelayKey);
            helper.BlendNumber(ProfilePropertyKeys.LightningProbabilityKey);
            helper.BlendNumber(ProfilePropertyKeys.LightningStrikeCoolDown);
            helper.BlendNumber(ProfilePropertyKeys.LightningIntensityKey);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(ProfilePropertyKeys.ThunderSoundVolumeKey);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(ProfilePropertyKeys.ThunderSoundVolumeKey);
        }
    }
}
