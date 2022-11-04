using System;
using System.Collections;
using System.Collections.Generic;
using RTG;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DirLightPanel : InfoPanel<DirLightPanel>
{
    public Transform LightParent;
    public Transform EnvParent;
    private Slider[] sliders;
    private Text[] texts;
    private Button defaultButton;
    private int lightSelect = -1;
    private List<GameObject> allColors = new List<GameObject>();
    private List<GameObject> allMoods = new List<GameObject>();
    private static int firstColorNum = 0;

    //range value
    private Vector2 intensityRange = new Vector2(0,3);

    //default vale
    private int defIntensity = 23;
    private int defAngle = 60;
    private int defDir = 333;
    private int defLightColorIndex = 0;
    private int defEnrColorIndex = 0;
    private Vector3 selectScale = new Vector3(0.66f, 0.66f, 0.66f);

    private DirLightBehaviour dirBehaviour;
    private SceneEntity lightEntity;
    private SceneEntity skyEntity;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        dirBehaviour = SceneBuilder.Inst.DirLight;
        skyEntity = SceneBuilder.Inst.SkyboxBev.entity;
        lightEntity = dirBehaviour.entity;
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
                OnDirChange(value);
            });

        defaultButton = transform.Find("Panel/Button").GetComponent<Button>();
        defaultButton.onClick.AddListener(SetDefault);
        InitLightColor();
    }

    public void SetSkyID(int id,float intensity,float anglex,float angley)
    {
        skyEntity.Get<SkyboxComponent>().skyboxId = id;
        sliders[0].value = TranslateSliderByRealIntensity(intensity);
        sliders[1].value = anglex;
        sliders[2].value = angley;
        InitMoodSelect();
        InitColorSelect();
    }

    private void InitLightColor()
    {
        var itemPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "DirLightColorItem");
        for (int i = 0; i < AssetLibrary.Inst.lightLib.Size(); i++)
        {
            int index = i;
            var itemGo = GameObject.Instantiate(itemPrefab, LightParent);
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

        for (int i = 0; i < AssetLibrary.Inst.enrLib.Size(); i++)
        {
            int index = i;
            var itemGo = GameObject.Instantiate(itemPrefab, EnvParent);
            var bg = itemGo.transform.Find("bg").gameObject;
            bg.GetComponent<Image>().color = AssetLibrary.Inst.enrLib.Get(i).sky;
            itemGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnEnrColor(index);
            });
            var colorm = itemGo.transform.Find("sel").gameObject;
            colorm.GetComponent<Image>().color = AssetLibrary.Inst.enrLib.Get(i).sky;
            allMoods.Add(colorm);
        }
        InitMoodSelect();
        int selIndex = DataUtils.GetColorSelect(DataUtils.ColorToString(lightEntity.Get<DirLightComponent>().color), AssetLibrary.Inst.lightLib.List);
        SetSelect(selIndex, allColors);
    }


    private void OnIntensityChange(float val)
    {
        float real = TranslateIntensity(val);
        dirBehaviour.SetIntensity(TranslateIntensity(val));
        lightEntity.Get<DirLightComponent>().intensity = real;
    }

    private void OnAngleChange(float val)
    {
        var anglex = val > 90 ? 0 : val;
        dirBehaviour.SetAngleX(anglex);
        lightEntity.Get<DirLightComponent>().anglex = anglex;
    }

    private void OnDirChange(float val)
    {
        dirBehaviour.SetAngleY(val);
        lightEntity.Get<DirLightComponent>().angley = val;
    }

    private void OnLightColor(int index)
    {
        var color = AssetLibrary.Inst.lightLib.Get(index);
        dirBehaviour.SetColor(color);
        lightEntity.Get<DirLightComponent>().color = color;
        SetSelect(index, allColors);
    }

    private void OnEnrColor(int index)
    {
        var gradient = AssetLibrary.Inst.enrLib.Get(index);
        //昼夜天空盒时，只允许进行编辑操作，但不实际影响天空盒效果
        if (skyEntity.Get<SkyboxComponent>().skyboxType != SkyboxType.DayNight)
        {
            SkyboxBehaviour.SetSkyGradient(AmbientMode.Trilight,gradient.sky,gradient.equator,gradient.ground);
        }

        SetSelect(index, allMoods);
    }

    private float TranslateIntensity(float intensity)
    {
        return intensity * (intensityRange.y - intensityRange.x) / 100 + intensityRange.x;
    }

    private int TranslateSliderByRealIntensity(float realInten)
    {
        return (int)((realInten - intensityRange.x) * 100f / (intensityRange.y - intensityRange.x));
    }

    public void SetDefault()
    {
        if (skyEntity.Get<SkyboxComponent>().skyboxType == SkyboxType.DayNight)
        {
            //昼夜天空盒还原, 恢复到默认天空盒的参数
            var data = GameConsts.settings[SkyboxManager.Inst.defaultSkyId];
            sliders[0].value = TranslateSliderByRealIntensity(data.intensity);
            sliders[1].value = data.anglex;
            sliders[2].value = data.angley;
            texts[0].text = ((int)sliders[0].value).ToString();
            texts[1].text = ((int)sliders[1].value).ToString() + "º";
            texts[2].text = ((int)sliders[2].value).ToString() + "º";
            var dirComp = lightEntity.Get<DirLightComponent>();
            dirComp.anglex = data.anglex;
            dirComp.angley = data.angley;
            dirComp.intensity = data.intensity;
            dirComp.color = data.dirctional;
        }
        else
        {
            var skyId = skyEntity.Get<SkyboxComponent>().skyboxId;
            var data =  GameConsts.settings[skyId];
            sliders[0].value = TranslateSliderByRealIntensity(data.intensity);
            sliders[1].value = data.anglex;
            sliders[2].value = data.angley;
            texts[0].text = ((int)sliders[0].value).ToString();
            texts[1].text = ((int)sliders[1].value).ToString() + "º";
            texts[2].text = ((int)sliders[2].value).ToString() + "º";
            var dirComp = lightEntity.Get<DirLightComponent>();
            dirComp.anglex = data.anglex;
            dirComp.angley = data.angley;
            dirComp.intensity = data.intensity;
            dirComp.color = data.dirctional;
            SkyboxBehaviour.SetSkySetting(skyId);
            SkyboxBehaviour.SetSkyboxCubemap(skyId);
        }
        
        BasePrimitivePanel.DisSelect();
        InitColorSelect();
        InitMoodSelect();
    }

    private void SetSelect(int index,List<GameObject> gameObjects)
    {
        GameUtils.SetSelect(index, gameObjects);
    }
    /// <summary>
    /// 取消选中态
    /// </summary>
    /// <param name="gameObjects"></param>
    private void CancelSelect(List<GameObject> gameObjects)
    {
        gameObjects.ForEach(x =>
       {
           x.SetActive(false);
           var item = x.transform.parent;
           if (item)
           {
               var bg = item.transform.Find("bg").gameObject;
               bg.transform.localScale = Vector3.one;
           }
       });
    }
    /// <summary>
    /// 设置Color选中态
    /// </summary>
    private void InitColorSelect()
    {
        bool isSelect = false;
        string curColor = DataUtils.ColorToString(dirBehaviour.GetComponentInChildren<Light>().color);
        for (int i = 0; i < AssetLibrary.Inst.lightLib.Size(); i++)
        {
            string color = DataUtils.ColorToString(AssetLibrary.Inst.lightLib.Get(i));
            if (color == curColor)
            {
                isSelect = true;
                SetSelect(i, allColors);
            }
        }
        if (!isSelect)
        {
            CancelSelect(allColors);
        }
    }
    /// <summary>
    /// 设置Mood选中态
    /// </summary>
    private void InitMoodSelect()
    {
        bool isSelect = false;
        string curColor = DataUtils.ColorToString(RenderSettings.ambientSkyColor);
        for (int i = 0; i < AssetLibrary.Inst.enrLib.Size(); i++)
        {
            string color = DataUtils.ColorToString(AssetLibrary.Inst.enrLib.Get(i).sky);
            if (color == curColor)
            {
                isSelect = true;
                SetSelect(i, allMoods);
            }
        }
        if (!isSelect)
        {
            CancelSelect(allMoods);
        }
    }
}
