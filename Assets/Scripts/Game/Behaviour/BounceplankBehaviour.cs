/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/7/26 21:12:25
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceplankBehaviour : NodeBaseBehaviour
{
    private List<GameObject> bounceplanks;
    private List<GameObject> Bounceplanks
    {
        get
        {
            if (bounceplanks == null)
            {
                bounceplanks = new List<GameObject>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    bounceplanks.Add(transform.GetChild(i).gameObject);
                }
            }
            return bounceplanks;
        }
    }
    private Animator animator;
    public Animator Anim
    {
        get
        {
            if (animator == null)
            {
                animator = gameObject.GetComponentInChildren<Animator>();
            }
            return animator;
        }
    }
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
    public int GetHeight()
    {
        return (int)GetHeightEnum();
    }
    public BounceHeight GetHeightEnum()
    {
        return (BounceHeight)System.Enum.Parse(typeof(BounceHeight), entity.Get<BounceplankComponent>().BounceHeight);
    }
   
    public void PlayJumpAnim()
    {
        Anim.Play(entity.Get<BounceplankComponent>().BounceHeight);
        
    }
    public void SetTiling(Vector2 tiling)
    {
        if (Renderers.Length <= 0)
        {
            return;
        }
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.SetVector("_Maintex_ST", new Vector4(tiling.x, tiling.y, 0, 0));
        }
        entity.Get<BounceplankComponent>().tile = tiling;
    }
    public void SetShape(BounceShape shape)
    {
        HideAllShape();
        entity.Get<BounceplankComponent>().shape = (int)shape;
        animator = null;
        switch (shape)
        {
            case BounceShape.Round:
                Bounceplanks[0].SetActive(true);
                return;
            case BounceShape.Square:
                Bounceplanks[1].SetActive(true);
                return;
        }
    }
    public void HideAllShape()
    {
        for (int i = 0; i < Bounceplanks.Count; i++)
        {
            Bounceplanks[i].SetActive(false);
        }
    }
    public void SetHeight(BounceHeight height)
    {
        entity.Get<BounceplankComponent>().BounceHeight = height.ToString();
    }
    
    public void SetColor(Color color)
    {
        if (Renderers.Length <= 0)
            return;
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.SetColor("_color", color);
        }
        entity.Get<BounceplankComponent>().color = DataUtils.ColorToString(color);

    }
    public void SetMatetial(int id)
    {

        var matData = GameManager.Inst.matConfigDatas.Find(x=>x.id == id);
        Texture t = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName);
        for (int i = 0; i < Renderers.Length; i++)
        {
            Renderers[i].material.SetTexture("_Maintex", t);
        }
        entity.Get<BounceplankComponent>().mat = matData.id;
    }
    private Color[] oldColor;
    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        if (isHigh)
        {
            oldColor = new Color[Renderers.Length];
            for (int i = 0; i < oldColor.Length; i++)
            {

                oldColor[i] = Renderers[i].material.GetColor("_color");
                Color hColor = new Color(1.3f, 1.3f, 1.3f, 1);
                Renderers[i].material.SetColor("_color", hColor);

            }
        }
        else
        {
            if (oldColor != null && oldColor.Length == Renderers.Length)
            {
                for (int i = 0; i < oldColor.Length; i++)
                {
                    Renderers[i].material.SetColor("_color", oldColor[i]);
                }
            }
        }
    }
}
