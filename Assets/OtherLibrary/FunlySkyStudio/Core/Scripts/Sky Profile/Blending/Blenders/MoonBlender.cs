using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{

    public class MoonBlender : FeatureBlender
    {
        protected override string featureKey
        {
            get { return ProfileFeatureKeys.MoonFeature; }
        }

        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendColor(ProfilePropertyKeys.MoonColorKey);
            helper.BlendNumber(ProfilePropertyKeys.MoonSizeKey);
            helper.BlendNumber(ProfilePropertyKeys.MoonEdgeFeatheringKey);
            helper.BlendNumber(ProfilePropertyKeys.MoonColorIntensityKey);
            helper.BlendNumber(ProfilePropertyKeys.MoonAlpha);
            helper.BlendColor(ProfilePropertyKeys.MoonLightColorKey);
            helper.BlendNumber(ProfilePropertyKeys.MoonLightIntensityKey);
            helper.BlendSpherePoint(ProfilePropertyKeys.MoonPositionKey);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(ProfilePropertyKeys.MoonAlpha);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(ProfilePropertyKeys.MoonAlpha);
        }
    }
}