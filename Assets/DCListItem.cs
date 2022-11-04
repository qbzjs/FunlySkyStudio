using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DCListItem : MonoBehaviour
{
    public enum IconType
    {
        AirDrop = 1,
        DC
    }
    
    public enum StateType
    {
        Succ = 0,
        Processing = 1,
        Soldout,
        Claimed,
        TokenGated,
        RequirePayment,
    }
    
    public Image leftIcon;
    public Image mainIcon;
    public Text itemName;
    public Color disableColor;
    public Toggle tog;

    public GameObject soldOut;
    public GameObject processing;
    public GameObject tokenGate;
    public GameObject requirePayment;
    public GameObject claimed;

    public Text maxWidthText;
    
    public Sprite airDropIcon;
    public Sprite dcIcon;

    private DCItem dcItem;
    public void Init(DCItem dcItem, Web3Status web3Status)
    {
        mainIcon.gameObject.SetActive(false);
        this.dcItem = dcItem;
        itemName.text = TextExtension.GetTextWithEllipsisByWidth(maxWidthText, dcItem.name);
        LoadMainIcon(dcItem.resourceType, dcItem.resourceId);
        RefLeftIcon(dcItem.nftType);
        RefState(dcItem.publishStatus, web3Status);
        RefWeb3Status(web3Status);
    }

    private void RefWeb3Status(Web3Status web3Status)
    {
        if (web3Status == Web3Status.Close)
        {
            itemName.color = disableColor;
            tog.interactable = false;
        }
    }

    private void LoadMainIcon(int clsType, int id)
    {
        if (RoleConfigDataManager.Inst.ConfigDataDic.TryGetValue((ClassifyType)clsType, out var l))
        {
            var item = l.Find(m => m.id == id);
            if (item != null)
            {
                RoleConfigDataManager.Inst.SetAvatarIcon(mainIcon, item.spriteName, null,
                    (state) => mainIcon.gameObject.SetActive(state == ImgLoadState.Complete));
            }
        }
    }

    public DCItem GetDCItem()
    {
        return dcItem;
    }
    
    private void RefState(int tag, Web3Status web3Status)
    {
        
        if (tag > 0)
        {
            tog.isOn = false;
            tog.interactable = false;
            itemName.color = disableColor;
            soldOut.SetActive((int)StateType.Soldout == tag);
            processing.SetActive((int)StateType.Processing == tag);
            tokenGate.SetActive((int)StateType.TokenGated == tag);
            requirePayment.SetActive((int)StateType.RequirePayment == tag);
            claimed.SetActive((int)StateType.Claimed == tag);
        }
        else
        {
            if (web3Status == Web3Status.Open)
            {
                tog.isOn = true;
            }
        }
    }

    private void RefLeftIcon(int tag)
    {
        if (tag == (int)IconType.AirDrop)
        {
            leftIcon.sprite = airDropIcon;
        }
        else
        {
            leftIcon.sprite = dcIcon;
        }
    }
    
}
