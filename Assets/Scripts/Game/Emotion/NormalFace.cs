using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalFace : BaseFace
{
    public override void OnStop()
    {
        base.OnStop();
        if (nextEmoInfo != null)
        {
            FaceArgs faceArgs = new FaceArgs(anim, emoIconData.nextEmoInfo, defultName, roleCon);
            OnPlay(faceArgs);
        }
        else
        {
            OnKill();
        }
   
    }
    protected IEnumerator PlayFaceEmo()
    {
        yield return new WaitForSeconds(emoIconData.delateTime);
        roleCon.SetFacialDefaultPos();
        anim.Play(emoIconData.name, 1, 0f);
        if ((emoIconData.moveEndTime - emoIconData.faceEndTime) > 0.01f)
        {
            yield return new WaitForSeconds(emoIconData.faceEndTime - emoIconData.delateTime);
            roleCon.SetCustomDefaultPos();
            anim.Play(defultName, 1, 0f);
            yield return new WaitForSeconds(emoIconData.moveEndTime - emoIconData.faceEndTime);
        }
        else
        {
            yield return new WaitForSeconds(emoIconData.moveEndTime - emoIconData.delateTime);
        }
        OnStop();
    }
    protected IEnumerator PlayLoopFaceEmo()
    {
        yield return new WaitForSeconds(emoIconData.delateTime);
        roleCon.SetFacialDefaultPos();
        anim.Play(emoIconData.name, 1, 0f);
        if ((emoIconData.moveEndTime - emoIconData.faceEndTime) > 0.01f)
        {
            yield return new WaitForSeconds(emoIconData.faceEndTime - emoIconData.delateTime);
            roleCon.SetCustomDefaultPos();
            anim.Play(defultName, 1, 0f);
            yield return new WaitForSeconds(emoIconData.moveEndTime - emoIconData.faceEndTime);
        }
        else
        {
            yield return new WaitForSeconds(emoIconData.moveEndTime - emoIconData.delateTime);
        }
        PlayLoopEmo();
    }
    public override void PlayEmo()
    {
        PlayingFaceEmo = CoroutineManager.Inst.StartCoroutine(PlayFaceEmo());
    }

    public override void PlayLoopEmo()
    {
        PlayingFaceEmo = CoroutineManager.Inst.StartCoroutine(PlayLoopFaceEmo());
    }
 
}
