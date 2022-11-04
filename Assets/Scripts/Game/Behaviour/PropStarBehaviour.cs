/*
* @Author: YangJie
 * @LastEditors: wenjia
* @Description:
* @Date: ${YEAR}-${MONTH}-${DAY} ${TIME}
* @Modify:
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BudEngine.NetEngine;
using DG.Tweening;
using UnityEngine;

public class PropStarBehaviour : NodeBaseBehaviour
{

    private Color[] oldColor;
    private bool isEnter = false;
    private GameMode curGameMode;
    
    private Sequence animTweenSeq = null;
    private Coroutine waitNetCoroutine = null;
    public bool isCollected = false;
    private float originY;

    public bool CheckCanClick()
    {
        return isCanClick && !isCollected;
    }

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        HighLightUtils.HighLightOnSpecial(isHigh, gameObject, ref oldColor);
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        GetRootCombineOrSelf().DOKill(true);
        PortalPlayPanel.Hide();
        originY = GetRootCombineOrSelf().transform.position.y;
    }



    private void OnDestroy()
    {
        KillCollectAnim();
    }

    public override void OnReset()
    {
        KillCollectAnim();
        base.OnReset();
        isCanClick = true;
    }


    public void OnChangeMode(GameMode mode)
    {
        if (mode == GameMode.Edit)
        {
            EndPropStar();
        }
        else
        {
            StartPropStar();
        }
    }
    /// <summary>
    /// 仅函数代码封装
    /// </summary>
    public void EndPropStar()
    {
        GetRootCombineOrSelf().gameObject.SetActive(true);
        LoggerUtils.Log("animTweenSeq:" + (animTweenSeq == null));
        animTweenSeq?.Kill(true);
        animTweenSeq = null;
        ReSetStatus();
    }

    public void StartPropStar()
    {
        originY = GetRootCombineOrSelf().position.y;
        isCanClick = true;
        ReSetStatus();
    }


    public void SetCollect()
    {
        isCanClick = false;
        isCollected = true;
        GetRootCombineOrSelf().gameObject.SetActive(false);
    }



    void OnClickProp()
    {
        if (PlayerBaseControl.Inst != null && PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            //被冻结时候不能拾取星星
            return;
        }
        if (!isCanClick) return;


        isCanClick = false;
        isCollected = true;
        PortalPlayPanel.Hide();
        var tmpRoot = GetRootCombineOrSelf();
        var collectComps = tmpRoot.GetComponentsInChildren<PropStarBehaviour>();
        var entities = new List<SceneEntity>();
        
        foreach (var collectComp in collectComps)
        {
            collectComp.isCanClick = false;
            entities.Add(collectComp.entity);
        }
     
        PlayCollectAnim(tmpRoot, () =>
        {
            tmpRoot.gameObject.SetActive(false);
            CollectControlManager.Inst.CollectEntities(entities.ToArray());
        });
    }

    /// <summary>
    /// 获取父级 组合节点，若父级不是组合节点 则返回自己本身
    /// </summary>
    public Transform GetRootCombineOrSelf()
    {

        var tmpBehaviour = transform.parent.GetComponent<CombineBehaviour>();
        return tmpBehaviour != null  ? tmpBehaviour.transform : transform;
    }


    public void KillCollectAnim()
    {
        if (animTweenSeq != null)
        {
            animTweenSeq.Kill(true);
            animTweenSeq = null;
        }
        
        
    }

    void PlayCollectAnim(Transform tmpRoot, Action callBack)
    {

        if (tmpRoot.GetComponent<CombineBehaviour>() != null)
        {
            tmpRoot.DOKill();
            var children = tmpRoot.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                child.DOPause();
            }
        }

        animTweenSeq?.Kill();



        AKSoundManager.Inst.PostEvent("play_pickup_star", gameObject);
        if (!StateManager.IsOnSlide)
        {
            PlayerBaseControl.Inst.playerAnim.Play("pickup");
        }
        originY = tmpRoot.position.y;
        animTweenSeq = DOTween.Sequence();
        float dis = 0.1f;
        float moveTime = 0.2f;
        animTweenSeq.Append(tmpRoot.DOMoveY(tmpRoot.position.y + 0,moveTime + 1));//延迟
        // animTweenSeq.Append(compNode.DOLocalMoveY(compNode.position.y + dis,moveTime));
        // animTweenSeq.Append(compNode.DOLocalMoveY(compNode.position.y - dis,moveTime));
        animTweenSeq.Append(tmpRoot.DOMoveY(tmpRoot.position.y + dis,moveTime));
        animTweenSeq.Append(tmpRoot.DOMoveY(tmpRoot.position.y - dis,moveTime));
        animTweenSeq.Append(tmpRoot.DOMoveY(tmpRoot.position.y + dis*8,0.35f));
        animTweenSeq.AppendCallback(()=>
        {
            animTweenSeq = null;
            LoggerUtils.Log("播放完毕");

            callBack?.Invoke();
            tmpRoot.position = new Vector3(tmpRoot.position.x, originY, tmpRoot.position.z);
        });
    }

    private void ReSetStatus()
    {

        
        isCollected = false;
        var tmpRoot = GetRootCombineOrSelf();

        tmpRoot.position = new Vector3(tmpRoot.position.x, originY, tmpRoot.position.z);
        
    }

    public override void OnRayEnter()
    {
        if (!isCanClick) return;
        HighLight(true);
        isEnter = true;
        PortalPlayPanel.Show();
        PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Collect);
        PortalPlayPanel.Instance.AddButtonClick(OnClickProp);
        PortalPlayPanel.Instance.SetTransform(transform);
    }

    public override void OnRayExit()
    {
        if (isCanClick == false) return;
        HighLight(false);
        isEnter = false;
        PortalPlayPanel.Hide();
    }
    
}