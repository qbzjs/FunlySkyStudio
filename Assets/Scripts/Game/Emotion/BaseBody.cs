using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBody
{
    protected Animator anim;
    protected AnimationController animCon;
    protected EmoIconData emoIconData;
    protected bool isLoop;
    protected EmoIconData nextEmoInfo;
    protected Coroutine PlayingMove;

   
    public virtual void OnPlay(BodyArgs args) {
        anim = args.animCon.playerAnim;
        animCon = args.animCon;
        emoIconData = args.emoIconData;
        isLoop = args.emoIconData.isBodyLoop == 1;
        nextEmoInfo = args.emoIconData.nextEmoInfo;


        if (isLoop)
        {
            PlayLoopEmo();
        }
        else
        {
            PlayEmo();
        }
    }

    public virtual void OnStop() {
    }
    public virtual void OnKill() {
        if (PlayingMove!=null)
        {
            CoroutineManager.Inst.StopCoroutine(PlayingMove);
            PlayingMove = null;
        }
        
    }

    public virtual void PlayLoopEmo()
    {
        
    }

    public virtual void PlayEmo()
    {
       
    }
}

public class BodyArgs
{
    public AnimationController animCon;
    public EmoIconData emoIconData;
    public bool isLoop;
    public string nextEmoName;
    public BodyArgs(AnimationController animCon, EmoIconData emoIconData)
    {
        this.animCon = animCon;
        this.emoIconData = emoIconData;

    }
}


