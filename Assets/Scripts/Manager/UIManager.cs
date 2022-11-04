using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoManager<UIManager>
{
    private Transform _canvas;

    public Transform uiCanvas
    {
        get
        {
            if (_canvas == null)
            {
                _canvas = GameObject.Find("Canvas/Panel").transform;
            }

            return _canvas;
        }
    }

    private Transform globalCanvas
    {
        get
        {
            if (_canvas == null)
            {
                _canvas = GameObject.Find("Canvas/Panel").transform;
            }

            return _canvas.parent;
        }
    }

    private readonly List<BasePanel> dialogList = new List<BasePanel>();

    public T CreateDialog<T>(bool isGlobal) where T : BasePanel
    {
        var prefab = GetDialogPrefab<T>();
        var uiParent = isGlobal ? globalCanvas : uiCanvas;
        var dialog = Instantiate(prefab, uiParent);
        dialog.OnInitByCreate();
        return dialog;
    }

    private T GetDialogPrefab<T>() where T : BasePanel
    {
        string path = typeof(T).Name;
        //TODO:wait for modify
        var dialogObj = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath +  path);
        var dialog = dialogObj.GetComponent<T>();
        if (dialog != null)
            return dialog;
        throw new MissingReferenceException("Prefab not found for type " + typeof(T));
    }

    public void OpenDialog(BasePanel instance)
    {
        instance.transform.SetAsLastSibling();
        if (!dialogList.Contains(instance))
            dialogList.Add(instance);
        instance.OnDialogBecameVisible();
    }

    public void CloseDialog(BasePanel instance)
    {
        if (dialogList.Count == 0)
        {
            return;
        }

        if (dialogList.Contains(instance))
        {
            instance.OnBackPressed();
            instance.gameObject.SetActive(false);
            dialogList.Remove(instance);
            if (instance.DestroyWhenClosed)
            {
                DestroyImmediate(instance.gameObject);
            }
        }
    }

    public void CloseAllDialog()
    {
        dialogList.ForEach(x =>
        {
            if (x != null)
            {
                x.gameObject.SetActive(false);
            }
        });
        dialogList.Clear();
    }
    
    private Dictionary<BasePanel, bool> stateList;
    public void SwitchDialog()
    {
        if (stateList == null)
        {
            stateList = new Dictionary<BasePanel, bool>();
            dialogList.ForEach(x =>
            {
                if (x != null)
                {
                    stateList.Add(x,x.gameObject.activeSelf);
                    x.gameObject.SetActive(false);
                }
            });
        }
        else
        {
            foreach (var kv in stateList)
            {
                if (!kv.Value || kv.Key is TipPanel) continue;
                kv.Key.gameObject.SetActive(kv.Value);
            }
            stateList = null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        dialogList.Clear();
    }

    public BasePanel GetPanelByName(string panelName)
    {
        if (dialogList != null && dialogList.Count > 0)
        {
            foreach (var dialog in dialogList)
            {
                if (dialog.GetType().ToString() == panelName)
                {
                    return dialog;
                }
            }
        }
        return null;
    }
    

}