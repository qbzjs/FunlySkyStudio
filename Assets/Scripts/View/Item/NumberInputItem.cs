using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Author : Tee Li
/// 描述：整数输入与加减按钮组合Item
/// 日期：2022/10/10
/// </summary>
public class NumberInputItem : MonoBehaviour
{
    public Text txt;
    public Button plusBtn;
    public Button subBtn;
    public Button inputBtn;
    public int CurVal { get; private set; }

    private UnityAction<int> onValueChange;
    private Predicate<int> inputVarify;
    private UnityAction<int> onInvalidInput;

    private void Awake()
    {
        InitOnAwake();
    }

    public virtual void InitOnAwake()
    {
        AddBuildinListeners();
    }

    protected virtual void AddBuildinListeners()
    {
        plusBtn?.onClick.AddListener(OnAddClick);
        subBtn?.onClick.AddListener(OnSubClick);
        inputBtn?.onClick.AddListener(OnInputClick);
    }

    public void SetValue(int value)
    {
        if (inputVarify != null)
        {
            if (!inputVarify.Invoke(value))
            {
                onInvalidInput?.Invoke(value);
                return;
            }           
        }
        SetCurVal(value);
        onValueChange?.Invoke(value);
    }

    public void SetValueWithoutNotify(int value)
    {
        if (inputVarify != null)
        {
            if (!inputVarify.Invoke(value))
            {
                onInvalidInput?.Invoke(value);
                return;
            }
        }
        SetCurVal(value);
    }

    public void AddListener(UnityAction<int> onChange)
    {
        onValueChange = onChange;
    }

    public void AddInvalidListener(UnityAction<int> onInvalid)
    {
        onInvalidInput = onInvalid;
    }

    public void AddInputVerify(Predicate<int> verify)
    {
        inputVarify = verify;
    }

    private void SetCurVal(int value)
    {
        CurVal = value;
        txt.text = value.ToString();
    }


    private void OnAddClick()
    {
        int val = CurVal + 1;
        SetValue(val);
    }

    private void OnSubClick()
    {
        int val = CurVal - 1;
        SetValue(val);
    }

    private void OnInputClick()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = txt.text,
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = "Oops! Exceed limit:(",
            defaultText = "",
            returnKeyType = (int)ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, OnInputEnter);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    private void OnInputEnter(string input)
    {
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);

        if (string.IsNullOrEmpty(input))
        {
            return;
        }

        bool isInt = int.TryParse(input, out int value);
        if (isInt)
        {
            SetValue(value);
        }
        else
        {
            TipPanel.ShowToast("Please enter the correct value");
        }
    }


}
