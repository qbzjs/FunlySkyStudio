using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEffect
{
    protected Animator anim;
    protected EmoIconData emoIconData;
    protected GameObject playerModle;
    protected RoleController roleCon;
    protected bool isLoop;
    public List<GameObject> expressionGameObject = new List<GameObject>();
    protected Coroutine PlayingEffect;




    public virtual void OnPlay(EffectArgs args)
    {
        anim = args.anim;
        emoIconData = args.emoIconData;
        isLoop = args.emoIconData.isEffectLoop == 1;
        playerModle = args.playerModle;
        roleCon = args.roleCon;


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
        if (PlayingEffect != null)
        {
            CoroutineManager.Inst.StopCoroutine(PlayingEffect);
            PlayingEffect = null;
        }
        ClearExpression();
    }


    protected void CreateExpression()
    {
        ClearExpression();
        string path;
        string name = emoIconData.name.Split('_')[0];
        for (int i = 0; i < emoIconData.effectCount; i++)
        {
            if (emoIconData.effectCount <= 1)
            {
                path = "Prefabs/Emotion/Express/" + name;
            }
            else
            {
                path = "Prefabs/Emotion/Express/" + name + "_" + (i + 1);
            }
            GameObject movePrefab = ResManager.Inst.LoadCharacterRes<GameObject>(path);
            if (movePrefab != null && playerModle.activeSelf)
            {
                expressionGameObject.Add(GameObject.Instantiate(movePrefab));
                if (IsBandBody(i, emoIconData))
                {
                    expressionGameObject[i].transform.parent = playerModle.transform;
                    expressionGameObject[i].transform.localRotation = Quaternion.identity;
                    expressionGameObject[i].transform.localPosition = Vector3.zero;
                    expressionGameObject[i].transform.localScale = Vector3.one;
                }
                else
                {
                    expressionGameObject[i].transform.parent = roleCon.GetBandNode(emoIconData.bandBody[i].bandNode);
                    expressionGameObject[i].transform.localRotation = Quaternion.Euler(emoIconData.bandBody[i].r.x, emoIconData.bandBody[i].r.y, emoIconData.bandBody[i].r.z);
                    expressionGameObject[i].transform.localPosition = emoIconData.bandBody[i].p;
                    expressionGameObject[i].transform.localScale = emoIconData.bandBody[i].s;
                }
            }
            if (!string.IsNullOrEmpty(emoIconData.playEffectAnim) && i < expressionGameObject.Count)
            {
                var animator = expressionGameObject[i].GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play(emoIconData.playEffectAnim, 0, 0f);
                }
            }
        }
    }
    protected void ClearExpression()
    {
        if (expressionGameObject != null && expressionGameObject.Count > 0)
        {
            for (int i = 0; i < expressionGameObject.Count; i++)
            {
                GameObject.Destroy(expressionGameObject[i]);
            }
            expressionGameObject.Clear();
        }
    }
  
    public bool IsBandBody(int id, EmoIconData info)
    {
        if (info.bandBody != null)
        {
            for (int i = 0; i < info.bandBody.Length; i++)
            {
                if (info.bandBody[i].id == id)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public virtual void PlayLoopEmo()
    {

    }

    public virtual void PlayEmo()
    {

    }
}
public class EffectArgs
{
    public Animator anim;
    public EmoIconData emoIconData;
    public GameObject playerModle;
    public RoleController roleCon;
    public int randomId;
    public bool isLoop;
    public EffectArgs() { }
    public EffectArgs(Animator anim, EmoIconData emoIconData, GameObject playerModle, RoleController roleCon)
    {
        this.anim = anim;
        this.emoIconData = emoIconData;
        this.playerModle = playerModle;
        this.roleCon = roleCon;
    }
    public EffectArgs(Animator anim, EmoIconData emoIconData, GameObject playerModle, RoleController roleCon, int randomId)
    {
        this.anim = anim;
        this.emoIconData = emoIconData;
        this.playerModle = playerModle;
        this.roleCon = roleCon;
        this.randomId = randomId;
    }
}