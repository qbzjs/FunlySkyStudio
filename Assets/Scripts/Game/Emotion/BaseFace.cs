using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFace
{
    protected Animator anim;
    protected EmoIconData emoIconData;
    protected string defultName;
    protected RoleController roleCon;
    protected Coroutine PlayingFaceEmo;
    protected EmoIconData nextEmoInfo;
    protected bool isLoop;
    public virtual void OnPlay(FaceArgs args)
    {
        anim = args.anim;
        emoIconData = args.emoIconData;
        defultName = args.defultName;
        roleCon = args.roleCon;
        isLoop = args.emoIconData.isFaceLoop == 1;
        nextEmoInfo = args.emoIconData.nextEmoInfo;


        args.roleCon.SetCustomDefaultPos();
        args.anim.SetLayerWeight(2, 0f);


        if (isLoop)
        {
            PlayLoopEmo();
        }
        else
        {
            PlayEmo();
        }
    }
 
    public virtual void OnStop()
    {

    }

    public virtual void OnKill()
    {
        if (PlayingFaceEmo!=null)
        {
            CoroutineManager.Inst.StopCoroutine(PlayingFaceEmo);
            PlayingFaceEmo = null;
        }
        roleCon.SetCustomDefaultPos();
        anim.Play(defultName, 1, 0f);
    }

    public virtual void PlayLoopEmo()
    {
       
    }

    public virtual void PlayEmo()
    {
      
    }
}
public class FaceArgs
{
    public Animator anim;
    public EmoIconData emoIconData;
    public string defultName;
    public RoleController roleCon;
    public bool isLoop;
    public FaceArgs(Animator anim, EmoIconData emoIconData, string defultName, RoleController roleCon) {
        this.anim = anim;
        this.emoIconData = emoIconData;
        this.defultName = defultName;
        this.roleCon = roleCon;
    }

}
