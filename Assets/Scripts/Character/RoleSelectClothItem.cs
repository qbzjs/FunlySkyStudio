/// <summary>
/// Author:Zhouzihan
/// Description:
/// Date: 2022/6/30 18:18:21
/// </summary>
using UnityEngine;
using System;
using UnityEngine.UI;
using Newtonsoft.Json;
using SavingData;

public class RoleSelectClothItem : MonoBehaviour
{
    public Button StyleBtn;
    public RawImage iconImg;
    public GameObject colorSelectGo;
    private Action<RoleSelectClothItem> OnSelect;

    private Image StyleBg;
    private Color defColor = new Color32(177, 174, 255, 0);
    private Color selectColor = new Color32(255, 255, 255, 0);
    public string imgName;
    public string coverUrl;
    public RoleData roleData;

    protected void Start()
    {
        StyleBg = StyleBtn.GetComponent<Image>();
        StyleBtn.onClick.AddListener(OnSelectClick);

    }

    public void Init(RoleData data, Action<RoleSelectClothItem> select)
    {
        roleData = data;
        OnSelect = select;
        colorSelectGo.SetActive(false);
        SetIconImgVisible(false);
    }

    public void OnSelectClick()
    {
  
        OnSelect?.Invoke(this);
    }

    public void OnSelectMatchItem(RoleController roleComp)
    {
        
        var rData = roleData.Clone() as RoleData;
        
       
        roleComp.InitRoleByData(rData);
        DataLogUtils.LogWearByRoleData(rData);//上报DC试穿
        roleComp.StartEyeAnimation(rData.eId);

    }

    public void SetSelectState(bool isVisible)
    {
        colorSelectGo.SetActive(isVisible);
        StyleBtn.GetComponent<Image>().color = isVisible ? selectColor : defColor;
    }

    
    public void SendCancelMatchCollection()
    {
        MatchData matchData = new MatchData();
        matchData.name = imgName;
        matchData.coverUrl = coverUrl;
        matchData.data = JsonConvert.SerializeObject(roleData);
        HttpUtils.MakeHttpRequest("/image/delCollocation", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(matchData), OnCancelSaveSuccess, OnFail);

    }
    public void OnCancelSaveSuccess(string msg)
    {

    }



    public void OnFail(string err)
    {
        LoggerUtils.Log("delete match fail --- /image/delCollocation ---- errInfo: " + err);
    }

    public void UpdateIconImg(string imgUrl, Action callBack = null)
    {
        CoroutineManager.Inst.StartCoroutine(GameUtils.LoadTextureByQueue(imgUrl,
               (tex) =>
               {
                   var newTex = tex as Texture2D;
                   newTex.Compress(true);
                   iconImg.texture = newTex;
                   coverUrl = imgUrl;
                   SetIconImgVisible(true);
                   callBack?.Invoke();
               },
               (error) =>
               {
                   LoggerUtils.Log(error);
                   callBack?.Invoke();
               }));
    }

    public void SetIconImgVisible(bool visible)
    {
        iconImg.gameObject.SetActive(visible);
        
    }
}
