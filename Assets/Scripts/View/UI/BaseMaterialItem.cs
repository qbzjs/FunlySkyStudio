using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseMaterialItem : MonoBehaviour
{
    public GameObject select;
    public RawImage icon;
    public Button button;
    public UGCMatData uData;
    public Texture StoreTexture;
    UnityAction<string> action;
    public void Init(Action<UGCMatData> dataAction, UGCMatData data)
    {
        uData = data;
        button.onClick.AddListener(() =>dataAction(data));
    }
    public void SetItemTexture(Texture tex)
    {
        if (tex != null)
        {
            icon.texture = tex;
        }
    }

    public void SetSelectState(bool isVisible)
    {
        select.SetActive(isVisible);
       
    }
    public void SetStore(UnityAction<string> action)
    {
        this.action = action;
     
        icon.texture = StoreTexture;
        button.onClick.AddListener(OnOpenUGCMatStoreBtnClick);
    }
    public void OnOpenUGCMatStoreBtnClick()
    {
#if UNITY_EDITOR
        action("");
#else
           OpenStorePageData data = new OpenStorePageData()
        {
            dataType = (int)Data_Type.Material
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.closeNativePage, action);
        MobileInterface.Instance.OpenStorePage(JsonConvert.SerializeObject(data));
#endif
    }
}
