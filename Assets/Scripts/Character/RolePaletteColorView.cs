using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author:Meimei-LiMei
/// Description:色盘界面管理
/// Date: 2022/6/1 18:53:43
/// </summary>
public class RolePaletteColorView : MonoBehaviour
{
  public Button paletteBtn;
  public Button hsvBtn;
  public Button backBtn;
  public Button hsvBackBtn;
  public PaletteColorView paletteView;
  public RoleColorView roleColorView;
  public HsvColorView hsvView;
  public void Start()
  {  
    paletteBtn.onClick.AddListener(PaletteBtnClick);
    hsvBtn.onClick.AddListener(OnHsvBtnClick);
    backBtn.onClick.AddListener(OnBackBtnClick);
    hsvBackBtn.onClick.AddListener(OnBackHsvClick);
    //兼容非全面屏手机的按钮显示
    var screenRatio = (float)Screen.safeArea.height / (float)Screen.safeArea.width;//屏幕宽高比
    if (screenRatio < 1.8f)
    {
        backBtn.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 300, 0);
        paletteView.GetComponentInChildren<GridLayoutGroup>().padding.bottom=400;

        hsvBackBtn.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 300, 0);
    }
  }
  private void OnEnable()
  {
    if (paletteView.gameObject.activeSelf)
    {
      paletteView.gameObject.SetActive(false);
    }
    if (hsvView.gameObject.activeSelf)
    {
       hsvView.gameObject.SetActive(false);
    }
  }
  public void PaletteBtnClick()
  {
    paletteView.gameObject.SetActive(true);
    if (roleColorView.curItem != null)
    {
      paletteView.SetSelect(roleColorView.curItem.rcData);
    }
  }

  public void OnHsvBtnClick()
  {
     hsvView.gameObject.SetActive(true);
     hsvView.SetToCurrentTarget();
  }

    public void OnBackBtnClick()
  {
    paletteView.gameObject.SetActive(false);
    if (paletteView.paletteCurItem != null)
    {
      roleColorView.SetSelect(paletteView.paletteCurItem.rcData);
    }    
  }

  public void OnBackHsvClick()
  {
    hsvView.gameObject.SetActive(false);
    roleColorView.SetSelect(hsvView.GetCurrentColor());
    paletteView.SetSelect(hsvView.GetCurrentColor());
  }


}
