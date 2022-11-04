using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RedDotItem : MonoBehaviour
{
    public Image RedImage;

    private string RedDotKey;

    private void Start() 
    {
    }
    
    public void Init(string key,Vector2 vec2 = default)
    {
        RedDotKey = key;
        SetPos(vec2);
        RedDotManager.Inst.RegisterRedDot(RedDotKey,this);
    }
    public void SetPos(Vector2 vec2)
    {
        GetComponent<RectTransform>().anchoredPosition = vec2;
    }

    private void OnDestroy() 
    {
        RedDotManager.Inst.UnRegisterRedDot(RedDotKey);
    }
}
