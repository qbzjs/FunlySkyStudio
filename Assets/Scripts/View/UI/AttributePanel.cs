/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttributePanel : BasePanel<AttributePanel>
{
    public RectTransform AttributeLayScollRect;

    private PickablityPanel pickablityPanel;
    private EdibilityPanel edibilityPanel;
    private CatchabilityPanel catchabilityPanel;

    private void OnEnable()
    {
        Invoke("UpdateAttributePanel", 0.1f);
    }

    public void Init()
    {
        pickablityPanel = GetComponentInChildren<PickablityPanel>(true);
        edibilityPanel = GetComponentInChildren<EdibilityPanel>(true);
        catchabilityPanel = GetComponentInChildren<CatchabilityPanel>(true);
        UpdateAttributePanel();
    }

    public void SetEntity(SceneEntity curEntity)
    {
        var pickable = PickabilityManager.Inst.CheckCanSetPickability(curEntity);
        pickablityPanel.SetEntity(curEntity);
        pickablityPanel.gameObject.SetActive(pickable);

        var eatable = EdibilityManager.Inst.CheckEdibility(curEntity);
        edibilityPanel.SetEntity(curEntity);
        edibilityPanel.gameObject.SetActive(eatable);

        var catchable = FishingManager.Inst.CheckCanSetCatchability(curEntity);
        catchabilityPanel.SetEntity(curEntity);
        catchabilityPanel.gameObject.SetActive(catchable);

        UpdateAttributePanel();
    }

    public bool HasAttribute(SceneEntity curEntity)
    {
        var pickable = PickabilityManager.Inst.CheckCanSetPickability(curEntity);
        var eatable = EdibilityManager.Inst.CheckEdibility(curEntity);
        var catchable = FishingManager.Inst.CheckCanSetCatchability(curEntity);
        var hasAttr = pickable || eatable || catchable;
        return hasAttr;
    }

    public void UpdateAttributePanel()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(AttributeLayScollRect);
    }
}
