using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PointLightPanel:InfoPanel<PointLightPanel>
{
    public Transform LightParent;
    private Text[] texts;
    private Slider[] sliders;
    private List<GameObject> allColors = new List<GameObject>();
    private int matId;
    private int colorId;
    //range value
    private Vector2 intensityRange = new Vector2(0, 3);
    private Vector2 lightRange = new Vector2(2, 10);
    private Vector3 selectScale = new Vector3(0.66f, 0.66f, 0.66f);

    private PointLightBehaviour pointBehaviour;
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
                texts[1].text = ((int)value).ToString();
                OnRangeChange(value);
            }); 
        InitLightColor();
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
            itemGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnLightColor(index);
            });
            var colorm = itemGo.transform.Find("sel").gameObject;
            colorm.GetComponent<Image>().color = AssetLibrary.Inst.lightLib.Get(i);
            allColors.Add(colorm);
        }
    }

    public void SetEntity(SceneEntity entity)
    {
        pointBehaviour = entity.Get<GameObjectComponent>().bindGo.GetComponent<PointLightBehaviour>();
        var matComp = entity.Get<MaterialComponent>();
        matId = matComp.matId;
        colorId = pointBehaviour.entity.Get<PointLightComponent>().colorId;
        var selectIndex = GameManager.Inst.matConfigDatas.FindIndex(x => x.id == matId);
        int selIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(pointBehaviour.entity.Get<PointLightComponent>().color), AssetLibrary.Inst.lightLib.List);
        SetColorSelect(selIndex);
    }


    private void OnIntensityChange(float val)
    {
        float realVal = DataUtils.GetRealValue((int)val, intensityRange.x, intensityRange.y, 0, 100);
        pointBehaviour.SetIntensity(realVal);
        pointBehaviour.entity.Get<PointLightComponent>().Intensity = realVal;
    }

    private void OnRangeChange(float val)
    {
        float realVal = DataUtils.GetRealValue((int)val, lightRange.x, lightRange.y, 0, 100);
        pointBehaviour.SetRange(realVal);
        pointBehaviour.entity.Get<PointLightComponent>().Range = realVal;
    }

    private void OnLightColor(int index)
    {
        var color = AssetLibrary.Inst.lightLib.Get(index);
        pointBehaviour.SetColor(color);
        pointBehaviour.entity.Get<PointLightComponent>().color = color;
        pointBehaviour.entity.Get<PointLightComponent>().colorId = index;
        SetColorSelect(index);
    }


    public void SetInitArgs()
    {
        var pComp = pointBehaviour.entity.Get<PointLightComponent>();
        pointBehaviour.SetColor(pComp.color);
        sliders[0].value = DataUtils.GetProgress(pComp.Intensity, intensityRange.x, intensityRange.y, 0, 100);
        sliders[1].value = DataUtils.GetProgress(pComp.Range, lightRange.x, lightRange.y, 0, 100);
        texts[0].text = ((int)sliders[0].value).ToString();
        texts[1].text = ((int)sliders[1].value).ToString();
    }

    private void SetColorSelect(int index)
    {
        GameUtils.SetSelect(index, allColors);
    }
}