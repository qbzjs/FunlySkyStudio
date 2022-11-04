using System.Collections;
using System.Collections.Generic;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
    public class CloudBlender : FeatureBlender
    {
        protected override string featureKey
        {
            get { return ProfileFeatureKeys.CloudFeature; }
        }
        
        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumber(ProfilePropertyKeys.CloudDensityKey);
            helper.BlendNumber(ProfilePropertyKeys.CloudTextureTiling);
            helper.BlendNumber(ProfilePropertyKeys.CloudSpeedKey);
            helper.BlendNumber(ProfilePropertyKeys.CloudDirectionKey);
            helper.BlendNumber(ProfilePropertyKeys.CloudFadeAmountKey);
            helper.BlendNumber(ProfilePropertyKeys.CloudFadePositionKey);
            helper.BlendNumber(ProfilePropertyKeys.CloudAlpha);
            helper.BlendColor(ProfilePropertyKeys.CloudColor1Key);
            helper.BlendColor(ProfilePropertyKeys.CloudColor2Key);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(ProfilePropertyKeys.CloudAlpha);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(ProfilePropertyKeys.CloudAlpha);
        }
    }
}