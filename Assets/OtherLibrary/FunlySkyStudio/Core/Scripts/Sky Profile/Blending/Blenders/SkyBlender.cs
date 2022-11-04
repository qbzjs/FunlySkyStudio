using System.Collections;
using System.Collections.Generic;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
    public class SkyBlender : FeatureBlender
    {
        /// <summary>
        ///  Feature key is empty here, since these properties always exist (with exception to cubemaps)
        /// </summary>
        protected override string featureKey
        {
            get { return ""; }
        }

        protected override ProfileFeatureBlendingMode BlendingMode(
            ProfileBlendingState state,
            BlendingHelper helper)
        {
            // The sky background always exists, so let's just do a full blend.
            return ProfileFeatureBlendingMode.Normal;
        }

        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            // TODO - How do we handle the cubemap? Shader to blend the textures maybe and assign?
            
            helper.BlendColor(ProfilePropertyKeys.SkyLowerColorKey);
            helper.BlendColor(ProfilePropertyKeys.SkyMiddleColorKey);
            helper.BlendColor(ProfilePropertyKeys.SkyUpperColorKey);
            helper.BlendNumber(ProfilePropertyKeys.SkyMiddleColorPositionKey);
            helper.BlendNumber(ProfilePropertyKeys.HorizonTrasitionStartKey);
            helper.BlendNumber(ProfilePropertyKeys.HorizonTransitionLengthKey);
            helper.BlendNumber(ProfilePropertyKeys.StarTransitionStartKey);
            helper.BlendNumber(ProfilePropertyKeys.StarTransitionLengthKey);
            helper.BlendNumber(ProfilePropertyKeys.HorizonStarScaleKey);
            
            // Ambient lighting.
            helper.BlendColor(ProfilePropertyKeys.AmbientLightSkyColorKey);
            helper.BlendColor(ProfilePropertyKeys.AmbientLightEquatorColorKey);
            helper.BlendColor(ProfilePropertyKeys.AmbientLightGroundColorKey);
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            // Not needed, since sky should exist in both from/to profiles.
            
            // Ambient lighting.
            helper.BlendColor(ProfilePropertyKeys.AmbientLightSkyColorKey);
            helper.BlendColor(ProfilePropertyKeys.AmbientLightEquatorColorKey);
            helper.BlendColor(ProfilePropertyKeys.AmbientLightGroundColorKey);
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            // Not needed, since sky should exist in both from/to profiles.
            
            // Ambient lighting.
            helper.BlendColor(ProfilePropertyKeys.AmbientLightSkyColorKey);
            helper.BlendColor(ProfilePropertyKeys.AmbientLightEquatorColorKey);
            helper.BlendColor(ProfilePropertyKeys.AmbientLightGroundColorKey);
        }
    }
}
