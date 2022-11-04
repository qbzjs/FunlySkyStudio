
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
    public class SunBlender: FeatureBlender
    {
        protected override string featureKey => ProfileFeatureKeys.SunFeature;
    
        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendColor(ProfilePropertyKeys.SunColorKey);
            helper.BlendNumber(ProfilePropertyKeys.SunSizeKey);
            helper.BlendNumber(ProfilePropertyKeys.SunEdgeFeatheringKey);
            helper.BlendNumber(ProfilePropertyKeys.SunColorIntensityKey);
            helper.BlendNumber(ProfilePropertyKeys.SunAlpha);
            helper.BlendColor(ProfilePropertyKeys.SunLightColorKey);
            helper.BlendNumber(ProfilePropertyKeys.SunLightIntensityKey);
            helper.BlendSpherePoint(ProfilePropertyKeys.SunPositionKey);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(ProfilePropertyKeys.SunAlpha);
            helper.BlendNumberIn(ProfilePropertyKeys.SunLightIntensityKey);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(ProfilePropertyKeys.SunAlpha);
            helper.BlendNumberOut(ProfilePropertyKeys.SunLightIntensityKey);
        }
    }
}