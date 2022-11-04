using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funly.SkyStudio
{
    public class BlendingHelper
    {
        private ProfileBlendingState m_State;
        
        public BlendingHelper(ProfileBlendingState state)
        {
            m_State = state;
        }

        public void UpdateState(ProfileBlendingState state)
        {
            m_State = state;
        }
        
        // Get the color for a key from a specific sky profile using the state's timeOfDay.
        public Color ProfileColorForKey(SkyProfile profile, string key)
        {
            // FIXME - For "toProfile" the timeOfDay should probably be zero?
            float timeOfDay = profile == m_State.toProfile ? 0 : m_State.timeOfDay;
            return profile.GetGroup<ColorKeyframeGroup>(key).ColorForTime(timeOfDay);
        }
    
        // Get a float value for a property key.
        public float ProfileNumberForKey(SkyProfile profile, string key)
        {
            float timeOfDay = profile == m_State.toProfile ? 0 : m_State.timeOfDay;
            return profile.GetGroup<NumberKeyframeGroup>(key).NumericValueAtTime(timeOfDay);
        }

        // Get the SpherePoint from a sky profile by it's property key.
        public SpherePoint ProfileSpherePointForKey(SkyProfile profile, string key)
        {
            float timeOfDay = profile == m_State.toProfile ? 0 : m_State.timeOfDay;
            return profile.GetGroup<SpherePointKeyframeGroup>(key).SpherePointForTime(timeOfDay);
        }
        
        public void BlendColor(string key)
        {
            BlendColor(
                key,
                ProfileColorForKey(m_State.fromProfile, key),
                ProfileColorForKey(m_State.toProfile, key),
                m_State.progress);
        }

        public void BlendColorOut(string key)
        {
            BlendColor(
                key,
                ProfileColorForKey(m_State.fromProfile, key),
                ProfileColorForKey(m_State.fromProfile, key).Clear(),
                m_State.outProgress);
        }

        public void BlendColorIn(string key)
        {
            BlendColor(
                key,
                ProfileColorForKey(m_State.toProfile, key).Clear(),
                ProfileColorForKey(m_State.toProfile, key),
                m_State.inProgress);
        }

        public void BlendColor(string key, Color from, Color to, float progress)
        {
            var group = m_State.blendedProfile.GetGroup<ColorKeyframeGroup>(key);
            group.keyframes[0].color = Color.LerpUnclamped(
                from,
                to,
                progress);
        }

        public void BlendNumber(string key)
        {
            BlendNumber(
                key,
                ProfileNumberForKey(m_State.fromProfile, key),
                ProfileNumberForKey(m_State.toProfile, key),
                m_State.progress);
        }
        
        public void BlendNumberOut(string key, float toValue = 0)
        {
            BlendNumber(
                key,
                ProfileNumberForKey(m_State.fromProfile, key),
                toValue,
                m_State.outProgress);
        }
        
        public void BlendNumberIn(string key, float fromValue = 0)
        {
            BlendNumber(
                key,
                fromValue,
                ProfileNumberForKey(m_State.toProfile, key),
                m_State.inProgress);
        }
        
        public void BlendNumber(string key, float from, float to, float progress)
        {
            var group = m_State.blendedProfile.GetGroup<NumberKeyframeGroup>(key);
            group.keyframes[0].value = Mathf.Lerp(from, to, progress);
        }
        
        public void BlendSpherePoint(string key)
        {
            BlendSpherePoint(
                key,
                ProfileSpherePointForKey(m_State.fromProfile, ProfilePropertyKeys.MoonPositionKey),
                ProfileSpherePointForKey(m_State.toProfile, ProfilePropertyKeys.MoonPositionKey),
                m_State.progress);    
        }
        
        public void BlendSpherePoint(string key, SpherePoint from, SpherePoint to, float progress)
        {
            // Do a spherical interpolation between 2 directions.
            Vector3 point = Vector3.Slerp(
                from.GetWorldDirection(),
                to.GetWorldDirection(),
                progress);
            
            var group = m_State.blendedProfile.GetGroup<SpherePointKeyframeGroup>(key);
            group.keyframes[0].spherePoint = new SpherePoint(point.normalized);
        }
        
        public ProfileFeatureBlendingMode GetFeatureAnimationMode(String featureKey)
        {
            Boolean fromSupportsFeature = m_State.fromProfile.IsFeatureEnabled(featureKey);
            Boolean toSupportsFeature = m_State.toProfile.IsFeatureEnabled(featureKey);

            if (fromSupportsFeature && toSupportsFeature)
            {
                return ProfileFeatureBlendingMode.Normal;
            }
            
            if (fromSupportsFeature && !toSupportsFeature)
            {
                return ProfileFeatureBlendingMode.FadeFeatureOut;
            }
            
            if (!fromSupportsFeature && toSupportsFeature)
            {
                return ProfileFeatureBlendingMode.FadeFeatureIn;
            }

            return ProfileFeatureBlendingMode.None;
        }
    }
}
