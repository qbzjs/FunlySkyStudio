using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Bloom效果，性能原因暂时隐藏AO效果
/// </summary>
public class PostProcessingPanel : InfoPanel<PostProcessingPanel>
{
//    public Toggle[] PostToggles;
//    public GameObject[] PostPanels;
    public Button ResetBtn;
    public Slider BloomSlider;
    public Toggle BloomToggle;
//    public Toggle AmbientToggle;
    private List<Text> toggleTexts;
    private SceneEntity pEntity;
    private Image[] sliderImages;
    private Color inactiveColor = new Color(0.729f, 0.717f, 0.68f);
    private Color textColor = new Color(1,1,1,0.4f);
    private PostProcessBehaviour postBehaviour;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        toggleTexts = new List<Text>();
        sliderImages = BloomSlider.GetComponentsInChildren<Image>();
//        for (var i = 0; i < PostToggles.Length; i++)
//        {
//            int index = i;
//            PostToggles[i].onValueChanged.AddListener((value) => OnSubSelect(index,value));
//            var text = PostToggles[i].GetComponentInChildren<Text>();
//            toggleTexts.Add(text);
//        }
        BloomSlider.onValueChanged.AddListener(OnBloomIntensityChange);
        BloomToggle.onValueChanged.AddListener(OnBloomToggleClick);
//        AmbientToggle.onValueChanged.AddListener(OnAmbientToggleClick);
        ResetBtn.onClick.AddListener(OnResetClick);
//        PostToggles[0].isOn = true;
//        OnSubSelect(0,true);
    }
    public void SetEntity(SceneEntity entity)
    {
        pEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        postBehaviour = entityGo.GetComponent<PostProcessBehaviour>();
        SetComponentData();
    }


    private void OnResetClick()
    {
        PostProcessCreater.SetDefault(postBehaviour);
        SetComponentData();
    }

    private void SetComponentData()
    {
        var comp = pEntity.Get<PostProcessComponent>();
        BloomSlider.value = comp.bloomIntensity;
        BloomToggle.isOn = comp.bloomActive == 1;
//        AmbientToggle.isOn = comp.amActive == 1;
    }

    private void OnBloomIntensityChange(float value)
    {
        float roundValue = (float) System.Math.Round(value, 1);
        pEntity.Get<PostProcessComponent>().bloomIntensity = roundValue;
        postBehaviour.ChangeBloomIntensity(roundValue);
    }

    private void OnBloomToggleClick(bool value)
    {
        int val = value ? 1 : 0;
        pEntity.Get<PostProcessComponent>().bloomActive = val;
        postBehaviour.SetBloomActive(val);
        BloomSlider.interactable = value;
        for (var i = 0; i < sliderImages.Length; i++)
        {
            sliderImages[i].color = value ? Color.white : inactiveColor;
        }
    }

//    private void OnAmbientToggleClick(bool value)
//    {
//        int val = value ? 1 : 0;
//        pEntity.Get<PostProcessComponent>().amActive = val;
//        postBehaviour.SetAmActive(val);
//    }

//    private void OnSubSelect(int index,bool visible)
//    {
//        if (visible)
//        {
//            for (var i = 0; i < PostToggles.Length; i++)
//            {
//                toggleTexts[i].color = textColor;
//                PostPanels[i].SetActive(false);
//            }
//            toggleTexts[index].color = Color.white;
//            PostPanels[index].SetActive(true);
//        }
//    }
}
