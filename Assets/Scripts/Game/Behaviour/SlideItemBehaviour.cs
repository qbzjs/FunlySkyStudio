using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideItemBehaviour : ActorNodeBehaviour
{
    public SlidePipeBehaviour mRoot;//父节点
    private Renderer[] renderers;
    private Renderer[] Renderers
    {
        get
        {
            if (renderers == null)
            {
                renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            }
            return renderers;
        }
    }

    private MeshCollider[] _colliders;
    private MeshCollider[] mColliders
    {
        get
        {
            if (_colliders == null)
            {
                _colliders = gameObject.GetComponentsInChildren<MeshCollider>(true);
            }
            return _colliders;
        }
    }

    public bool IsHead => entity.Get<SlideItemComponent>().ItemIndex == 1;
    public bool IsTail
    { get {
            SlideItemComponent compt = entity.Get<SlideItemComponent>();
            
            return compt.ItemIndex == mRoot.GetItemCount() && compt.ItemIndex>1;
        }
    }
    private bool isSelect = false;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
    }
    public override void OnRayEnter()
    {
        if (!IsCanRayEnter())
        {
            return;
        }
        SlidePipeComponent slidePipeCompt = mRoot.entity.Get<SlidePipeComponent>();
        bool isShowIcon = false;
        if (slidePipeCompt.WayType ==(int)SlidePipePanel.EWayType.One)
        {
            isShowIcon = IsHead;
        }
        else
        {
            isShowIcon = IsHead || IsTail;
        }
        if (isShowIcon)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Slide);
            PortalPlayPanel.Instance.SetTransform(GetIconRoot());
            PortalPlayPanel.Instance.AddButtonClick(OnClick, true);
        }
    }
    private Transform GetIconRoot()
    {
        Transform iconRoot = transform;
        if (IsTail)
        {
            iconRoot = GameUtils.FindChildByName(transform,"EndNode");
        }
        iconRoot = (iconRoot == null) ? transform : iconRoot;
        return iconRoot;
    }
    public bool IsCanRayEnter()
    {
        if (mRoot == null) return false;
        if (!IsHead && !IsTail) return false;
        if (PlayerBaseControl.Inst != null && StateManager.IsOnSlide)
        {
            //滑梯状态不再判断
            return false;
        }
        if (StateManager.PlayerOnCar
           || (PlayerOnBoardControl.Inst && PlayerOnBoardControl.Inst.isOnBoard))
        {
            return false;
        }
        if (StateManager.IsOnLadder||StateManager.IsOnSeesaw||StateManager.IsOnSwing)
        {
            return false;
        }
        if (PlayerBaseControl.Inst!=null&&PlayerBaseControl.Inst.GetNoAbilityFlag(EObjAbilityType.Move))
        {
            return false;
        }
        return true;
    }
    public override void OnRayExit()
    {
        base.OnRayExit();
        PortalPlayPanel.Hide();
    }
    public void OnClick()
    {
        mRoot.OnClick(this);
    }
    public void UpdateModel(GameObject gameObject, int id)
    {
        gameObject.transform.SetParent(this.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localEulerAngles = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        assetObj = gameObject;
        var gameComp = entity.Get<GameObjectComponent>();
        gameComp.modId = id;

        renderers = null;

        var itemComp = entity.Get<SlideItemComponent>();
        SetMatetial(itemComp.MatId);
        SetColor(itemComp.Color);
        SetTiling(itemComp.Tile);
        RefreshHighLight();

        SlidePipeComponent pipeComp = mRoot.entity.Get<SlidePipeComponent>();
        SetItemVirtual(pipeComp.HideModel);
    }
    public void UpdateLayer(int wayType)
    {
        Collider[] cols = gameObject.GetComponentsInChildren<Collider>(true);
        string name = "Model";
        if (wayType == (int)SlidePipePanel.EWayType.One)
        {
            name = IsHead ? "Touch":"Model";
        }
        if (wayType == (int)SlidePipePanel.EWayType.Tow)
        {
            name = IsHead || IsTail ? "Touch" : "Model";
        }
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].gameObject.layer = LayerMask.NameToLayer(name);
        }
    }
    public void Clear()
    {
        
    }

    public void SetMatetial(int id)
    {
        var matData = GameManager.Inst.matConfigDatas.Find(x=>x.id == id);
        Texture t = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName);
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.SetTexture("_MainTex", t);
        }
        entity.Get<SlideItemComponent>().MatId = matData.id;
    }

    public void SetColor(Color color)
    {
        if (Renderers.Length <= 0)
            return;
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.SetColor("_Color", color);
        }
        entity.Get<SlideItemComponent>().Color = color;
    }

    public void SetTiling(Vector2 tiling)
    {
        if (Renderers.Length <= 0)
        {
            return;
        }
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.SetTextureScale("_MainTex", tiling);
        }
        entity.Get<SlideItemComponent>().Tile = tiling;
    }

    public void SetSelect(bool select)
    {
        HighLight(select);
        isSelect = select;
        //ShowOutline(isSelect);
    }

    //鎻忚竟
    public void ShowOutline(bool isShow)
    {
        float value = isShow == true ? 1 : 0;
        for(int i = 0; i < Renderers.Length;i++)
        {
            if(isShow){
                Renderers[i].material.EnableKeyword("_STROKE_ONOFF_ON");
            }else{
                Renderers[i].material.DisableKeyword("_STROKE_ONOFF_ON");
            }
        } 
    }

    public void SetItemVirtual(int HideModel)
    {
        bool isHide = (HideModel == 1);
        for(int i = 0; i < Renderers.Length;i++)
        {
            if(isHide){
                //Renderers[i].material.EnableKeyword("_OPACITY_ON");
                Renderers[i].material.SetFloat("_opacity", 0.5f);
            }else{
                //Renderers[i].material.DisableKeyword("_OPACITY_ON");
                Renderers[i].material.SetFloat("_opacity", 1.0f);
            }
        } 
    }

    public void SetRenderVisible(bool isVisible)
    {
        for(int i = 0; i < Renderers.Length;i++)
        {
            Renderers[i].enabled = isVisible;   
        } 
    }

    public void SetColliderTrigger(bool isTrigger)
    {
        for(int i = 0; i < mColliders.Length;i++)
        {
            if(isTrigger)
            {
                mColliders[i].convex = true;
                mColliders[i].isTrigger = true;
            }
            else{
                mColliders[i].isTrigger = false;
                mColliders[i].convex = false;
            }
        } 
    }

    public void RefreshHighLight()
    {
        HighLight(isSelect);
    }
    
    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        if (isHigh)
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                Color hColor = DataUtils.GetHighlightColor(entity.Get<SlideItemComponent>().Color);
                Renderers[i].material.SetColor("_Color", hColor);

            }
        }
        else
        {
            for (int i = 0; i < Renderers.Length; i++)
            {
                Renderers[i].material.SetColor("_Color", entity.Get<SlideItemComponent>().Color);
            }
        }
    }
}
