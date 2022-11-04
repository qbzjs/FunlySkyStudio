using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UGCClothesTextBehaviour : ElementBaseBehaviour
{
    public TextData data;
    public Text self;
    private string LastStr;
    private int maxLength = 80;
    private string enterText = "Enter text...";
    public string EnterText { get { return LocalizationConManager.Inst.GetLocalizedText(enterText); } }
    [HideInInspector] 
    public Transform mirParent;
    public UGCClothesTextBehaviour textDynamicMir;

    public override void OnCopy()
    {
        base.OnCopy();
        if (UGCClothesTextManager.Inst.IsReachMaximumValue())
        {
            return;
        }
        var copyObj = GameObject.Instantiate(gameObject, MainUGCResPanel.Inst.elementPanel);
        var behav = copyObj.GetComponent<UGCClothesTextBehaviour>();
        behav.transform.localPosition += copyOffset;
        behav.textDynamicMir = UGCClothesTextManager.Inst.CreatMirrorText(self,mirParent);
        behav.OnTransformChange();
        UGCClothesTextManager.Inst.AddText(behav);
        if (TransformInteractorController.Inst.interActor)
        {
            TransformInteractorController.Inst.interActor.Settup(behav.self.rectTransform, behav.OnTransformChange,behav.Init);
        }
        UGCClothesTextManager.Inst.AddCreateRecord(copyObj, (int)behav.type);
        behav.AddClickEvent();
    }

    public override GameObject GetDynamicMir()
    {
        if (textDynamicMir)
        {
            return textDynamicMir.gameObject;
        }
        return null;
    }

    public override void OnTransformChange()
    {
        if (textDynamicMir)
        {
            Copy(self, textDynamicMir.self);
        }
    }

    private void Copy(Text org,Text mir)
    {
        mir.rectTransform.localPosition = org.rectTransform.localPosition;
        mir.rectTransform.localScale = org.rectTransform.localScale;
        mir.rectTransform.sizeDelta = org.rectTransform.sizeDelta;
        mir.rectTransform.localEulerAngles = org.rectTransform.localEulerAngles;  
        mir.color = org.color;
        mir.text = org.text;
    }

    public override void SetSelfData()
    {
        data.pos = DataUtils.Vector3ToString(transform.localPosition);
        data.sizeDelta = DataUtils.Vector2ToString(self.rectTransform.sizeDelta);
        data.rot = DataUtils.Vector3ToString(transform.localEulerAngles);
        data.color = DataUtils.ColorToString(self.color);
        data.hierarchy = hierarchy;
        data.content = self.text;
    }

    public override void SetMirSiblingIndex()
    {
        if (textDynamicMir != null)
        {
            textDynamicMir.rectTrans.SetSiblingIndex(rectTrans.GetSiblingIndex());
        }
    }

    public override void Select()
    {
        base.Select();
        string str = self.text.Trim();
        str = str.Equals(EnterText) ? "" : LastStr;
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = EnterText,
            inputMode = 2,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int)ReturnType.Done
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }

    public void ShowKeyBoard(string str)
    {
        string tempStr = DataUtils.FilterNonStandardText(str);
        if (string.IsNullOrEmpty(tempStr.Trim()))
            tempStr = EnterText;
        data.content = tempStr;
        LastStr = tempStr;
        SetTextContent(tempStr);
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
        OnTransformChange();
    }

    private void SetTextContent(string str)
    {
        if (str.Length > maxLength)
        {
            string temp = str.Substring(0, maxLength);
            self.text = temp + "...";
            return;
        }
        self.text = str;
    }

    public void SetColor(Color col)
    {
        self.color = col;
        if (textDynamicMir)
        {
            textDynamicMir.self.color = col;
        }
        SetSelfData();
    }

    public override void OnDis()
    {
        base.OnDis();
        UGCClothesTextManager.Inst.ReomveText(this);
        if (textDynamicMir)
        {
            SecondCachePool.Inst.DestroyEntity(textDynamicMir.gameObject);
        }
        if (UGCClothesTextManager.Inst.textList.Count <= 0)
        {
            MainUGCResPanel.Inst.PaintBtn.isOn = true;
        }
    }

    public override void OnClick(GameObject obj)
    {
        base.OnClick(obj);
        MainUGCResPanel.Inst.SetColorSelect(self.color);
        if (PaintTool.Current.pType != PaintType.Text)
        {
            Toggle textToggle = MainUGCResPanel.Inst.textBtn;
            textToggle.SetIsOnWithoutNotify(true);
            PaintTool.Current.SetPaintType(PaintType.Text);
        }
    }

    public override GameObject ExcuteMirObj()
    {
        if (textDynamicMir)
        {
            return textDynamicMir.gameObject;
        }
        return null;
    }

    public override void RedoInfo()
    {
        UGCClothesTextManager.Inst.AddText(this);
    }

    public override void UndoInfo()
    {
        UGCClothesTextManager.Inst.ReomveText(this);
    }

    public override void Init()
    {
        base.Init();
        MainUGCResPanel.Inst.colorPinkerPanel.SetElementColor = UGCClothesTextManager.Inst.SetColor;
        MainUGCResPanel.Inst.SetElementColor = UGCClothesTextManager.Inst.SetColor;
        LastStr = data.content;
    }

    public void OnDestroy()
    {
        UGCClothesTextManager.Inst.ReomveText(this);
    }
}
