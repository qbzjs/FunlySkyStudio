using System.Collections;
using System.Collections.Generic;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
    // Blender used to animate stars in a sky profile transition.
    public class StarBlender : FeatureBlender
    {
        // The star layer that this blender is targeting (Sky Profiles support 3 star layers).
        [Range(1, 3)]
        public int starLayer;

        protected override string featureKey
        {
            get
            {
                return "StarLayer" + starLayer + "Feature";
            }
        }
        
        protected override void BlendBoth(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendColor(PropertyKeyForLayer(ProfilePropertyKeys.Star1ColorKey));
            helper.BlendNumber(PropertyKeyForLayer(ProfilePropertyKeys.Star1SizeKey));
            helper.BlendNumber(PropertyKeyForLayer(ProfilePropertyKeys.Star1RotationSpeedKey));
            helper.BlendNumber(PropertyKeyForLayer(ProfilePropertyKeys.Star1TwinkleAmountKey));
            helper.BlendNumber(PropertyKeyForLayer(ProfilePropertyKeys.Star1TwinkleSpeedKey));
            helper.BlendNumber(PropertyKeyForLayer(ProfilePropertyKeys.Star1EdgeFeatheringKey));
            helper.BlendNumber(PropertyKeyForLayer(ProfilePropertyKeys.Star1ColorIntensityKey));
        }

        protected override void BlendIn(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberIn(PropertyKeyForLayer(ProfilePropertyKeys.Star1SizeKey));
        }

        protected override void BlendOut(ProfileBlendingState state, BlendingHelper helper)
        {
            helper.BlendNumberOut(PropertyKeyForLayer(ProfilePropertyKeys.Star1SizeKey));
        }

        // Get a star property key with the correct layer ID so we can reuse this logic for all 3 layers.
        private string PropertyKeyForLayer(string key)
        {
            return key.Replace("Star1", "Star" + starLayer);
        }
    }
}