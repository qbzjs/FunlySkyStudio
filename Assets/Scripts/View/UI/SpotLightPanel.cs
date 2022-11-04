using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using RTG;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SpotLightPanel : InfoPanel<SpotLightPanel>
{
    public Transform LightParent;
    private Slider[] sliders;
    private Text[] texts;
    private int matId;
    private int colorId;
    private List<GameObject> allColors = new List<GameObject>();

    //range value
    private Vector2 intensityRange = new Vector2(0,8);
    private Vector2 lightRange = new Vector2(1,10);
    private Vector2 angleRange = new Vector2(1,100);
    private Vector3 selectScale = new Vector3(0.66f, 0.66f, 0.66f);
    //default vale
    private const float defIntensity = 4;
    private const float defAngle = 50;
    private const float defRange = 5.5f;
    private const int defColorIndex = 0;

    private SpotLightBehaviour spotBehaviour;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        sliders = this.GetComponentsInChildren<Slider>();
        texts = this.GetComponentsInChildren<Text>();
        sliders[0].onValueChanged.AddListener(
            (value) => {
                texts[0].text = ((int)value).ToString();
                OnIntensityChange(value);
            });
        sliders[1].onValueChanged.AddListener(
            (value) => {
                texts[1].text = ((int)value).ToString() + "º";
                OnAngleChange(value);
            });
        sliders[2].onValueChanged.AddListener(
            (value) => {
                texts[2].text = ((int)value).ToString() + "º";
                OnRangeChange(value);
            });

        InitLightColor();
    }


    public void SetEntity(SceneEntity entity)
    {
        spotBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponent<SpotLightBehaviour>();
        var matComp = entity.Get<MaterialComponent>();
        matId = matComp.matId;
        colorId = spotBehaviour.entity.Get<SpotLightComponent>().colorId;
        var selectIndex = GameManager.Inst.matConfigDatas.FindIndex(x => x.id == matId);
        int selIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(spotBehaviour.entity.Get<SpotLightComponent>().color), AssetLibrary.Inst.lightLib.List);
        SetColorSelect(selIndex);
    }


    private void InitLightColor()
    {
        var itemPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "DirLightColorItem");
        for (int i = 0; i < AssetLibrary.Inst.lightLib.Size(); i++)
        {
            int index = i;
            var itemGo = GameObject.Instantiate(itemPrefab, LightParent);
            itemGo.GetComponent<Image>().color = AssetLibrary.Inst.lightLib.Get(i);
            var bg = itemGo.transform.Find("bg").gameObject;
            bg.GetComponent<Image>().color = AssetLibrary.Inst.lightLib.Get(i);
            itemGo.GetComponent<Button>().onClick.AddListener(()=>
            {
                OnLightColor(index);
            });
            var colorm = itemGo.transform.Find("sel").gameObject;
            colorm.GetComponent<Image>().color = AssetLibrary.Inst.lightLib.Get(i);
            allColors.Add(colorm);
        }

    }


    private void OnIntensityChange(float val)
    {
        float realVal = DataUtils.GetRealValue((int) val, intensityRange.x, intensityRange.y, 0, 100);
        spotBehaviour.SetIntensity(realVal);
        spotBehaviour.entity.Get<SpotLightComponent>().Intensity = realVal;
    }

    private void OnAngleChange(float val)
    {
        float realVal = DataUtils.GetRealValue((int)val, angleRange.x, angleRange.y, 0, 100);
        spotBehaviour.SetAngleY(realVal);
        spotBehaviour.entity.Get<SpotLightComponent>().SpotAngle = realVal;
    }

    private void OnRangeChange(float val)
    {
        float realVal = DataUtils.GetRealValue((int)val, lightRange.x, lightRange.y, 0, 100);
        spotBehaviour.SetRange(realVal);
        spotBehaviour.entity.Get<SpotLightComponent>().Range = realVal;
    }

    private void OnLightColor(int index)
    {
        var color = AssetLibrary.Inst.lightLib.Get(index);
        spotBehaviour.SetColor(color);
        spotBehaviour.entity.Get<SpotLightComponent>().color = color;
        spotBehaviour.entity.Get<SpotLightComponent>().colorId = index;
        SetColorSelect(index);
    }


    public void SetInitArgs()
    {
        var sComp = spotBehaviour.entity.Get<SpotLightComponent>();
        spotBehaviour.SetColor(sComp.color);
        sliders[0].value = DataUtils.GetProgress(sComp.Intensity, intensityRange.x,intensityRange.y,0,100);
        sliders[1].value = DataUtils.GetProgress(sComp.SpotAngle, angleRange.x, angleRange.y, 0, 100); ;
        sliders[2].value = DataUtils.GetProgress(sComp.Range, lightRange.x, lightRange.y, 0, 100);
        texts[0].text = ((int)sliders[0].value).ToString();
        texts[1].text = ((int)sliders[1].value).ToString() + "º";
        texts[2].text = ((int)sliders[2].value).ToString() + "º";
    }

    private void SetColorSelect(int index)
    {
        GameUtils.SetSelect(index, allColors);
    }
}
