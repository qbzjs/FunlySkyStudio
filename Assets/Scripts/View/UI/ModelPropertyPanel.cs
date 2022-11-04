using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelPropertyPanel : BasePanel<ModelPropertyPanel>
{
    public Toggle[] SubToggles;
    public GameObject[] Panels;
    public Button CloseBtn;
    private List<Text> toggleNames;
    private SceneEntity curEntity;
    private PropertyAnimPanel animPanel;
    private MovementPropertyPanel movePanel;
    private PropertyDefaultSetPanel defaultSetPanel;
    private TransactionPropPanel transactionPanel;
    private AttributePanel attributePanel;
    private GameObject animToggle;
    private GameObject moveToggle;
    private GameObject defaultToggle;
    private GameObject transacToggle;
    private GameObject attributeToggle;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        InputReceiver.locked = true;

        CloseBtn.onClick.AddListener(() =>
        {
            InputReceiver.locked = false;
            Hide();
            RestorePanels();
        });

        toggleNames = new List<Text>();
        for (var i = 0; i < SubToggles.Length; i++)
        {
            int index = i;
            toggleNames.Add(SubToggles[i].GetComponentInChildren<Text>());
            SubToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    SelectSub(index);
                }
            });
        }

        InitPropertyPanels();

        animToggle = Array.Find(SubToggles, a => a.name == "AnimToggle").gameObject;
        moveToggle = Array.Find(SubToggles, a => a.name == "MoveToggle").gameObject;
        defaultToggle = Array.Find(SubToggles, a => a.name == "DefaultStateToggle").gameObject;
        transacToggle = Array.Find(SubToggles, a => a.name == "TransacToggle").gameObject;
        attributeToggle = Array.Find(SubToggles, a => a.name == "AttributeToggle").gameObject;
    }

    private void InitPropertyPanels()
    {
        animPanel = Panels[0].GetComponent<PropertyAnimPanel>();
        animPanel.Init();
        movePanel = Panels[1].GetComponent<MovementPropertyPanel>();
        movePanel.Init();
        defaultSetPanel = Panels[2].GetComponent<PropertyDefaultSetPanel>();
        defaultSetPanel.Init();
        transactionPanel = Panels[3].GetComponent<TransactionPropPanel>();
        transactionPanel.Init();
        attributePanel = Panels[4].GetComponent<AttributePanel>();
        attributePanel.Init();
    }

    private void SelectSub(int index)
    {
        for (var i = 0; i < Panels.Length; i++)
        {
            Panels[i].SetActive(false);
            toggleNames[i].color = Color.white;
        }
        Panels[index].SetActive(true);
        toggleNames[index].color = Color.black;
    }

        
    public void SetEntity(SceneEntity entity)
    {
        curEntity = entity;
        InitDefaultPanel();
        InitTransactionToggle();
        InitAttributeToggle();
        InitSubToggles();
    }

    private void InitSubToggles()
    {
        foreach (var toggle in SubToggles)
        {
            if (toggle.gameObject.activeSelf)
            {
                toggle.isOn = true;
                var index = Array.FindIndex(SubToggles, a => a.name == toggle.name);
                SelectSub(index);
                return;
            }
        }
    }

    private void InitDefaultPanel()
    {
        var gameComp = curEntity.Get<GameObjectComponent>();
        var modeType = gameComp.modelType;
        var canSet = modeType != NodeModelType.FishingModel;
        moveToggle.SetActive(canSet);
        movePanel.SetEntity(curEntity);
        animToggle.SetActive(canSet);
        animPanel.SetEntity(curEntity);
        defaultToggle.SetActive(canSet);
        defaultSetPanel.SetEntity(curEntity);
    }

    private void InitTransactionToggle()
    {
        bool isTransactionVisible = IsTransactionVisible(curEntity);
        transacToggle.SetActive(isTransactionVisible);
        transactionPanel.SetEntity(curEntity);
    }

    private bool IsTransactionVisible(SceneEntity entity)
    {
        bool isTransactionVisible = false;
        var gameComp = curEntity.Get<GameObjectComponent>();
        if (gameComp.type == ResType.UGC)
        {
            isTransactionVisible = true;
            if (curEntity.HasComponent<BloodPropComponent>())
            {
                isTransactionVisible = false;
            } 
            if (curEntity.HasComponent<FreezePropsComponent>())
            {
                isTransactionVisible = false;
            }
            if (curEntity.HasComponent<FireworkComponent>())
            {
                isTransactionVisible = false;
            }
        }
        else if (curEntity.HasComponent<UGCClothItemComponent>())
        {
            isTransactionVisible = true;
        }
        else if (curEntity.HasComponent<PGCSceneComponent>())
        {
            isTransactionVisible = true;
        }
        return isTransactionVisible;
    }

    private void InitAttributeToggle()
    {
        attributePanel.SetEntity(curEntity);
        var hasAttr = attributePanel.HasAttribute(curEntity);
        attributeToggle.SetActive(hasAttr);
    }

    public void RefreshAttributePanel()
    {
        InitAttributeToggle();
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        animPanel.ResetSelectItems();
    }

    public void CloseFollowMode()
    {
        movePanel.anchorToggle.isOn = true;
        movePanel.followToggle.isOn = false;
    }

    /**
    * 恢复之前的界面和选中态
    */
    public void RestorePanels()
    {
        UIManager.Inst.uiCanvas.gameObject.SetActive(true);
        if (ReferManager.Inst.isRefer)
        {
            ReferPanel.Instance.OnReferMode();
        }
    }
}
