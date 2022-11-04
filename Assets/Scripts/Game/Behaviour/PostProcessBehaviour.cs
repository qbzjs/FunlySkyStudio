using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessBehaviour : NodeBaseBehaviour
{
    private PostProcessProfile profile;
    private PostProcessLayer postLayer;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        profile = this.GetComponent<PostProcessVolume>().profile;
        var cam = GameObject.Find("MainCamera");
        cam.TryGetComponent(out postLayer);
    }

    public void SetBloomActive(int isActive)
    {
        if (profile.GetSetting<Bloom>() != null)
            profile.GetSetting<Bloom>().active = isActive == 1;
    }

    //    public void SetAmActive(int isActive)
    //    {
    //        profile.GetSetting<AmbientOcclusion>().active = isActive == 1;
    //    }

    public void ChangeBloomIntensity(float inte)
    {
        if (profile.GetSetting<Bloom>() != null)
            profile.GetSetting<Bloom>().intensity.value = inte;
    }

    public void SetPostProcessActive(bool isActive)
    {
        
        GlobalFieldController.isOpenPostProcess = isActive;
        if (postLayer != null)
        {
            postLayer.enabled = isActive;
        }
        this.GetComponent<PostProcessVolume>().enabled = isActive;
    }
}