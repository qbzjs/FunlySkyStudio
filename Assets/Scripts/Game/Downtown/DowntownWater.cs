using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DowntownWater : MonoBehaviour
{
    private void OnEnable()
    {
        AkSoundEngine.PostEvent("Play_GreatSnowfield_Lake_Loop", this.gameObject);
    }

    private void OnDisable()
    {
        AkSoundEngine.StopAll(this.gameObject);
    }
}
