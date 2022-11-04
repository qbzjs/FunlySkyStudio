using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public abstract class BasePanel<T> : BasePanel where T : BasePanel<T>
{
    public static T Instance { get; private set; }
    // Execute before OnInitByCreate,Do not use as much as possible
    protected virtual void Awake()
    {
    }

    protected virtual void OnDestroy()
    {
        Instance = null;
    }

    public static void Show(bool isGlobal = false)
    {
        Open(isGlobal);
    }

    public static void Hide()
    {
        Close();
    }

    protected static void Open(bool isGlobal)
    {
        if (Instance == null)
            Instance = UIManager.Inst.CreateDialog<T>(isGlobal);
        else
            Instance.gameObject.SetActive(true);

        UIManager.Inst.OpenDialog(Instance);
    }

    protected static void Close()
    {
        if (Instance == null)
        {
            return;
        }
        UIManager.Inst.CloseDialog(Instance);
    }

    public override void OnBackPressed()
    {
    }

    public override void OnDialogBecameVisible()
    {
    }

    public override void OnInitByCreate()
    {
    }
}

public abstract class BasePanel : MonoBehaviour
{
    [Tooltip("Destroy the Game Object when dialog is closed (reduces memory usage)")]
    public bool DestroyWhenClosed = false;

    public abstract void OnInitByCreate();
    public abstract void OnDialogBecameVisible();
    public abstract void OnBackPressed();
}
