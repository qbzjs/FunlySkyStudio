using UnityEngine;
using System;
using UnityEngine.UI;
using Newtonsoft.Json;
using SavingData;

/// <summary>
/// Author:WenJia
/// Description: 搭配保存 Item
/// Date: 2022/4/25 17:38:12
/// </summary>


public class RoleMatchItem : MonoBehaviour
{
    public Button StyleBtn;
    public RawImage iconImg;
    public GameObject iconBg;
    public GameObject colorSelectGo;
    private Action<RoleMatchItem> OnSelect;
    public Action<GameObject> OnLongPress;
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

        var longPressBtn = StyleBtn.GetComponent<RoleItemLongPress>();
        longPressBtn.OnLongPress = OnLongPressItem;
        longPressBtn.scrollRect = transform.GetComponentInParent<ScrollRect>();
    }

    public void Init(RoleData data, Action<RoleMatchItem> select)
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

    public void OnSelectMatchItem()
    {
        var roleComp = RoleMenuView.Ins.rController;
        var rData = roleData.Clone() as RoleData;
        RoleConfigDataManager.Inst.SetRoleData(rData);
        RoleMenuView.Ins.UpdateRoleData();
        roleComp.InitRoleByData(rData);
        roleComp.StartEyeAnimation(rData.eId);
        RoleClassifiyView.Ins.ResetCurViewItemSelect();
        DataLogUtils.LogWearByRoleData(rData);
    }

    public void SetSelectState(bool isVisible)
    {
        colorSelectGo.SetActive(isVisible);
        StyleBtn.GetComponent<Image>().color = isVisible ? selectColor : defColor;
    }

    public void OnLongPressItem()
    {
        CharacterPopupPanel.Show();
        CharacterPopupPanel.Instance.SetItemData(this);
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
        var savesView = RoleMenuView.Ins.GetView<SavesView>();
        savesView.RemoveMatchItem(this);
    }

    public void OnFail(string err)
    {
        LoggerUtils.Log("delete match fail --- /image/delCollocation ---- errInfo: " + err);
    }

    public void UpdateIconImg(string imgUrl, Action callBack= null)
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
        iconBg.SetActive(visible);
    }
}

