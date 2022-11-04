using System.Collections;
using System.Collections.Generic;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
    /// <summary>
    ///  FeatureBlender defines an API for how features are animated during a Sky Profile transition.
    /// You Can subclass FeatureBlender and implement the functions to create a custom animation
    /// behavior. After you create your custom sky feature subclass, just make sure you update
    /// the "Sky Profile Transition Prefab" to reference you're custom animation.
    /// </summary>
    public abstract class FeatureBlender : MonoBehaviour, IFeatureBlender
    {
        protected abstract string featureKey { get; }
        
        /// <summary>
        /// Blend a feature that exists in both fromProfile and toProfile.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="helper"></param>
        protected abstract void BlendBoth(ProfileBlendingState state, BlendingHelper helper);

        /// <summary>
        /// Blend in feature that only exists in toProfile (but not in fromProfile).
        /// </summary>
        /// <param name="state"></param>
        /// <param name="helper"></param>
        protected abstract void BlendIn(ProfileBlendingState state, BlendingHelper helper);

        /// <summary>
        /// Blend out feature that only exists in fromProfile (but not in toProfile).
        /// </summary>
        /// <param name="state"></param>
        /// <param name="helper"></param>
        protected abstract void BlendOut(ProfileBlendingState state, BlendingHelper helper);
        
        /// <summary>
        /// Determine the best blending mode to use for this feature. This is decided typically
        /// by looking at if the same feature exists in both the fromProfile and toProfile.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        protected virtual ProfileFeatureBlendingMode BlendingMode(
            ProfileBlendingState state,
            BlendingHelper helper)
        {
            return helper.GetFeatureAnimationMode(featureKey);
        }
        
        public virtual void Blend(ProfileBlendingState state, BlendingHelper helper)
        {
            switch (BlendingMode(state, helper))
            {
                case ProfileFeatureBlendingMode.Normal:
                    BlendBoth(state, helper);
                    break;
                case ProfileFeatureBlendingMode.FadeFeatureOut:
                    BlendOut(state, helper);
                    break;
                case ProfileFeatureBlendingMode.FadeFeatureIn:
                    BlendIn(state, helper);
                    break;
                default:
                    break;
            }
        }
        
    }
}