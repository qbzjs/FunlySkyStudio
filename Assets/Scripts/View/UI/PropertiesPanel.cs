using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertiesPanel : BasePanel<PropertiesPanel>
{

    public Button ShowProBtn;
    private SceneEntity curEntity;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        ShowProBtn.onClick.AddListener(() =>
        {
            UIManager.Inst.uiCanvas.gameObject.SetActive(false);
            ModelPropertyPanel.Show(true);
            ModelPropertyPanel.Instance.SetEntity(curEntity);
        });

    }

    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
    }
}
