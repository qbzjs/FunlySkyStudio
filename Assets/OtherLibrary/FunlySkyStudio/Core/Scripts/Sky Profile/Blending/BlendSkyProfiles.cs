using System;
using UnityEngine;

namespace Funly.SkyStudio
{
    /// <summary>
    /// YOU DON'T NEED TO USE THIS DIRECTLY.
    /// This script is used to drive the Sky Profile transition animation. You don't need to directly use this,
    /// instead use the API on TimeOfDayController.instance.StartSkyProfileTransition(myNewSkyProfile).
    /// </summary>
    public class BlendSkyProfiles : MonoBehaviour
    {
        /// <summary>
        /// Profile we're blending away from.
        /// </summary>
        public SkyProfile fromProfile { get; private set; }
        
        /// <summary>
        /// Profile we're blending towards.
        /// </summary>
        public SkyProfile toProfile { get; private set; }
        
        /// <summary>
        /// Profile that contains the current blend for this animation step.
        /// </summary>
        public SkyProfile blendedProfile { get; private set; }

        /// <summary>
        /// Called when the blending completes.
        /// </summary>
        [Tooltip("Called when blending finishes.")]
        public Action<BlendSkyProfiles> onBlendComplete;
        
        [HideInInspector]
        private float m_StartTime = -1;
        
        [HideInInspector]
        private float m_EndTime = -1;

        [Tooltip("Blender used for basic sky background properties.")]
        public FeatureBlender skyBlender;

        [Tooltip("Blender used for the sun properties.")]
        public FeatureBlender sunBlender;
        
        [Tooltip("Blender used moon properties.")]
        public FeatureBlender moonBlender;
        
        [Tooltip("Blender used cloud properties.")]
        public FeatureBlender cloudBlender;
        
        [Tooltip("Blender used star layer 1 properties.")]
        public FeatureBlender starLayer1Blender;
        
        [Tooltip("Blender used star layer 2 properties.")]
        public FeatureBlender starLayer2Blender;
        
        [Tooltip("Blender used star layer 3 properties.")]
        public FeatureBlender starLayer3Blender;

        [Tooltip("Blender used by the rain downfall feature.")]
        public FeatureBlender rainBlender;

        [Tooltip("Blender used by the rain splash feature.")]
        public FeatureBlender rainSplashBlender;

        [Tooltip("Blender used for lightning feature properties.")]
        public FeatureBlender lightningBlender;

        [Tooltip("Blender used for fog properties.")]
        public FeatureBlender fogBlender;
        
        // Cache the percent values so we don't keep recalculating them.
        private bool m_IsBlendingFirstHalf = true;
        private ProfileBlendingState m_State;
        private TimeOfDayController m_TimeOfDayController;
        
        // Blending helper used for common interpolations.
        private BlendingHelper blendingHelper;
        

        /// <summary>
        /// Start blending the from a sky profile to another one.
        /// </summary>
        /// <param name="fromProfile"></param>
        /// <param name="toProfile"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public SkyProfile StartBlending(TimeOfDayController controller, SkyProfile fromProfile, SkyProfile toProfile, float duration)
        {
            if (controller == null)
            {
                Debug.LogWarning("Can't transition with null TimeOfDayController");
                return null;
            }
            
            if (fromProfile == null)
            {
                Debug.LogWarning("Can't transition to null 'from' sky profile.");
                return null;
            }

            if (toProfile == null)
            {
                Debug.LogWarning("Can't transition to null 'to' sky profile");
                return null;
            }
            
            // Check for cubemap blending which isn't supported fully yet.
            if (!fromProfile.IsFeatureEnabled(ProfileFeatureKeys.GradientSkyFeature) ||
                !toProfile.IsFeatureEnabled(ProfileFeatureKeys.GradientSkyFeature))
            {
                Debug.LogWarning("Sky Studio doesn't currently support automatic transition blending with cubemap backgrounds.");    
            }
            
            m_TimeOfDayController = controller;
            
            this.fromProfile = fromProfile;
            this.toProfile = toProfile;
            
            m_StartTime = Time.time;
            m_EndTime = m_StartTime + duration;

            // Create a copy and start it at the "from" profile.
            blendedProfile = Instantiate(fromProfile);
            blendedProfile.skyboxMaterial = fromProfile.skyboxMaterial;
            m_TimeOfDayController.skyProfile = blendedProfile;

            // Snapshot our state, we'll just update the progress as we animate to avoid allocations.
            m_State = new ProfileBlendingState(
                blendedProfile,
                fromProfile,
                toProfile,
                0,
                0,
                0,
                m_TimeOfDayController.timeOfDay);

            blendingHelper = new BlendingHelper(m_State);
            
            UpdateBlendedProfile();
            
            return blendedProfile;
        }

        public void CancelBlending()
        {
            TearDownBlending();
        }

        public void TearDownBlending()
        {
            if (m_TimeOfDayController == null)
            {
                return;
            }
            
            m_TimeOfDayController = null;
            blendedProfile = null;
            
            // Stop updating, and destroy the game object when done.
            enabled = false;
            Destroy(gameObject);
        }

        private void Update()
        {
            if (blendedProfile == null)
            {
                return;
            }
            
            UpdateBlendedProfile();
        }
        
        private void UpdateBlendedProfile()
        {
            if (m_TimeOfDayController == null)
            {
                return;
            }
            
            float duration = (m_EndTime - m_StartTime);
            float elapsed = (Time.time - m_StartTime);

            m_State.progress = elapsed / duration;
            m_State.inProgress = PercentForMode(ProfileFeatureBlendingMode.FadeFeatureIn, m_State.progress);
            m_State.outProgress = PercentForMode(ProfileFeatureBlendingMode.FadeFeatureOut, m_State.progress);
            
            blendingHelper.UpdateState(m_State);
            
            // Flip to destination material on second half to pickup new shader features since they may not exist in previous profile.
            if (m_State.progress > 0.5f && m_IsBlendingFirstHalf)
            {
                m_IsBlendingFirstHalf = false;
                blendedProfile = Instantiate(toProfile);
                m_State.blendedProfile = blendedProfile;
                m_TimeOfDayController.skyProfile = blendedProfile;
            }
            
            blendingHelper.UpdateState(m_State);

            FeatureBlender[] blenders = {
                skyBlender,
                sunBlender,
                moonBlender,
                cloudBlender,
                starLayer1Blender,
                starLayer2Blender,
                starLayer3Blender,
                rainBlender,
                rainSplashBlender,
                lightningBlender,
                fogBlender
            };
            
            // Run the custom profile blender for each major feature.
            foreach (FeatureBlender blender in blenders)
            {
                if (blender == null)
                {
                    continue;
                }

                blender.Blend(m_State, blendingHelper);
            }

            // Sky time is always zero while we blend since there is only 1 keyframe per group.
            m_TimeOfDayController.skyProfile = blendedProfile;

            // Check if we're done.
            if (m_State.progress >= 1.0f)
            {
                onBlendComplete(this);
                TearDownBlending();
            }
        }

        private float PercentForMode(ProfileFeatureBlendingMode mode, float percent)
        {
            switch (mode)
            {
                case ProfileFeatureBlendingMode.FadeFeatureIn:
                    return Mathf.Clamp01((percent - 0.5f) * 2.0f);
                case ProfileFeatureBlendingMode.FadeFeatureOut:
                    return Mathf.Clamp01(percent * 2.0f);
                default:
                    return percent;
            }
        }
        

        
    }
}
