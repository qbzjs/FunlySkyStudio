using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Author:WenJia
/// Description:色盘打开所有颜色的展示面板
/// Date: 2022/5/30 21:17:32
/// </summary>

public class PaletteColorView : MonoBehaviour
{
  public Transform colorParent;
  public GameObject colorItem;
  public Action<string> OnSelect;
  [HideInInspector]
  public RoleColorItem paletteCurItem;
  private List<RoleColorItem> paletteitems = new List<RoleColorItem>();
  public void InitPaletteView(List<string> colors, Action<string> select)
  {
    OnSelect = select;
    for (int i = 0; i < colors.Count; i++)
    {
      var go = Instantiate(colorItem, colorParent);
      var goScript = go.GetComponent<RoleColorItem>();
      goScript.Init(colors[i], this.OnSelectClick);
      paletteitems.Add(goScript);
    }
  }
  public void OnSelectClick(RoleColorItem item)
  {
    if (paletteCurItem == item)
    {
      return;
    }
    if (paletteCurItem != null)
    {
      paletteCurItem.SetSelectState(false);
    }
    paletteCurItem = item;
    paletteCurItem.SetSelectState(true);
    OnSelect?.Invoke(paletteCurItem.rcData);
    }
  public void SetSelect(string hexColor)
  {
    bool isExist = false;
    paletteitems.ForEach(x =>
    {
      x.SetSelectState(false);
        if (x.rcData.Equals(hexColor) || x.rcData.Equals(hexColor.ToLower()))
      {
        paletteCurItem=x;
        paletteCurItem.SetSelectState(true);
        OnSelect?.Invoke(paletteCurItem.rcData);
        isExist = true;
      }
    });
    if (!isExist)
    {
        paletteCurItem = null;
    }
  }
}
