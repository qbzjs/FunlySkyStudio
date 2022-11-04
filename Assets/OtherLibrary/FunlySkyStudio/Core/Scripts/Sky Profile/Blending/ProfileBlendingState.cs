using System.Collections;
using System.Collections.Generic;
using Funly.SkyStudio;
using UnityEngine;

namespace Funly.SkyStudio
{
    public struct ProfileBlendingState
    {
        public SkyProfile blendedProfile;
        public SkyProfile fromProfile;
        public SkyProfile toProfile;
        
        /// <summary>
        /// Progress value for the entire animation.
        /// </summary>
        public float progress;
        
        /// <summary>
        /// Progress value of the first half of the animation, where things are faded "out".
        /// </summary>
        public float outProgress;
        
        /// <summary>
        /// Progress value of the second half of the animation, where new things are faded "in".
        /// </summary>
        public float inProgress;
        
        /// <summary>
        /// Snapshot of of the timeOfDay at the start of the transition.
        /// </summary>
        public float timeOfDay;

        public ProfileBlendingState(
            SkyProfile blendedProfile,
            SkyProfile fromProfile,
            SkyProfile toProfile,
            float progress,
            float outProgress,
            float inProgress,
            float timeOfDay)
        {
            this.blendedProfile = blendedProfile;
            this.fromProfile = fromProfile;
            this.toProfile = toProfile;
            this.progress = progress;
            this.inProgress = inProgress;
            this.outProgress = outProgress;
            this.timeOfDay = timeOfDay;
        }
    }
}