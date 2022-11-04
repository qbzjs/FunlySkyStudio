using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UGCClothesTextManager : CInstance <UGCClothesTextManager>
{
    public Text textPrefabs;
    public List<UGCClothesTextBehaviour> textList = new List<UGCClothesTextBehaviour>();
    public float textScalingRatio = 3;
    public float minSize = 414f;
    private int maxNum = 99;
    private Action UndoRedoAct;
    public void Init()
    {
        textPrefabs = MainUGCResPanel.Inst.textPrefabs;
        UndoRedoAct = MainUGCResPanel.Inst.UpdateUndoBtnView;
    }

    public void AddText(UGCClothesTextBehaviour obj)
    {
        if (!textList.Contains(obj))
        {
            textList.Add(obj);
        }
    }

    public void ShowTextByIndex(int part)
    {
        for (var i = 0; i < textList.Count; i++)
        {
            textList[i].gameObject.SetActive(textList[i].part == part);
            textList[i].textDynamicMir.gameObject.SetActive(true);
        }
    }

    public void ReomveText(UGCClothesTextBehaviour obj)
    {
        if (textList.Contains(obj))
        {
            textList.Remove(obj);
        }
    }

    public List<TextData> GetPartTextDatas(int part)
    {
        List<TextData> tDatas = new List<TextData>();
        for (int i = 0; i < textList.Count; i++)
        {
            if(textList[i].part == part)
            {
                textList[i].SetSelfData();
                tDatas.Add(textList[i].data);
            }
        }
        return tDatas;
    }

    public bool IsReachMaximumValue()
    {
        if(textList.Count >= maxNum)
        {
            TipPanel.ShowToast("Oops! Only 99 texts can be added.");
            return true;
        }
        return false;
    }

    public UGCClothesTextBehaviour CreatText(int ClothesIndex, TextData tData,Transform par)
    {
        if (IsReachMaximumValue())
        {
            return null;
        }
        var textGo = GameObject.Instantiate(textPrefabs, MainUGCResPanel.Inst.elementPanel);
        var behav = textGo.gameObject.AddComponent<UGCClothesTextBehaviour>();
        behav.self = textGo;
        behav.part = ClothesIndex;
        behav.rectTrans = textGo.rectTransform;
        behav.type = BehaviourType.Text;
        behav.minSize = minSize;
        behav.scalingRatio = textScalingRatio;
        behav.AddClickEvent();
        behav.data = new TextData();
        SetData(tData, behav);
        var collider = textGo.gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(textGo.rectTransform.sizeDelta.x, textGo.rectTransform.sizeDelta.y, 0);
        AddText(behav);
        behav.mirParent = par;
        behav.textDynamicMir = CreatMirrorText(textGo,par);
        behav.OnTransformChange();
        return behav;
    }

    public UGCClothesTextBehaviour CreatMirrorText(Text textGo,Transform par)
    {
        var mirrorText = GameObject.Instantiate(textGo, par);
        mirrorText.rectTransform.localPosition = textGo.rectTransform.localPosition;
        mirrorText.rectTransform.localScale = textGo.rectTransform.localScale;
        mirrorText.rectTransform.sizeDelta = textGo.rectTransform.sizeDelta;
        mirrorText.rectTransform.localEulerAngles = textGo.rectTransform.localEulerAngles;
        mirrorText.gameObject.SetActive(true);
        return mirrorText.GetComponent<UGCClothesTextBehaviour>();
    }

    public void SetData(TextData tData, UGCClothesTextBehaviour behav)
    {
        var textGo = behav.self;
        if (tData != null)
        {
            behav.data = tData;
            textGo.rectTransform.localPosition = DataUtils.DeSerializeVector3(behav.data.pos);
            textGo.rectTransform.sizeDelta = DataUtils.DeSerializeVector2(behav.data.sizeDelta);
            textGo.rectTransform.localEulerAngles = DataUtils.DeSerializeVector3(behav.data.rot);
            textGo.rectTransform.localScale = Vector3.one;
            textGo.color = DataUtils.DeSerializeColor(behav.data.color);
            textGo.text = tData.content;
            behav.hierarchy = behav.data.hierarchy;
            textGo.gameObject.SetActive(true);
        }
        else
        {
            textGo.rectTransform.localPosition = Vector3.zero;
            textGo.rectTransform.localScale = Vector3.one;
            textGo.rectTransform.sizeDelta = new Vector2(minSize,minSize / textScalingRatio);
            textGo.rectTransform.localEulerAngles = Vector3.zero;
            textGo.color = Color.black;
            textGo.text = behav.EnterText;
            behav.hierarchy = behav.data.hierarchy;
            textGo.gameObject.SetActive(true);
        }
        behav.SetSelfData();
    }

    public void OnChangePart(int curPart)
    {
        for (int i = 0; i < textList.Count; i++)
        {
            if(textList[i].part == curPart)
            {
                textList[i].gameObject.SetActive(true);
            }
            else
            {
                textList[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetColor(Color col)
    {
        var tarObj = TransformInteractorController.Inst.interActor.targetGameObject;
        if (tarObj != null)
        {
            var behav = tarObj.GetComponent<UGCClothesTextBehaviour>();
            if (behav && col != behav.self.color)
            {
                var colorBeginData = CreateUndoData(UGCElementType.Color, behav.rectTrans, MainUGCResPanel.Inst.curSelectPart, DataUtils.ColorToString(behav.self.color));
                behav.SetColor(col);
                var colorEndData = CreateUndoData(UGCElementType.Color, behav.rectTrans, MainUGCResPanel.Inst.curSelectPart, DataUtils.ColorToString(behav.self.color));
                AddRecord(colorBeginData, colorEndData);
            }
        }
    }

    public void AddCreateRecord(GameObject gameObject, int type)
    {
        if (gameObject == null)
        {
            return;
        }
        UGCClothesCreateDestroyUndoData beginData = new UGCClothesCreateDestroyUndoData();
        beginData.targetNode = null;
        beginData.createUndoMode = (int)CreateUndoMode.Create;
        beginData.type = type;
        beginData.selectPart = MainUGCResPanel.Inst.curSelectPart;

        UGCClothesCreateDestroyUndoData endData = new UGCClothesCreateDestroyUndoData();
        endData.targetNode = gameObject;
        endData.createUndoMode = (int)CreateUndoMode.Create;
        endData.type = type;
        endData.selectPart = MainUGCResPanel.Inst.curSelectPart;

        UndoRecord record = new UndoRecord(UndoHelperName.UGCClothesCreateDestroyUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
        UndoRedoAct?.Invoke();
    }

    private ElementUndoData CreateUndoData(UGCElementType type, RectTransform rectTrans, GameObject selectPart, string color = "")
    {
        ElementUndoData data = new ElementUndoData();
        data.color = color;
        data.targetNode = rectTrans;
        data.transformType = (int)type;
        data.selectPart = selectPart;
        return data;
    }

    public void SetColorUndo(RectTransform targetTrans, Color col)
    {
        if (targetTrans == null) { return; }
        var behav = targetTrans.GetComponent<UGCClothesTextBehaviour>();
        if (behav)
        {
            behav.self.color = col;
            behav.OnTransformChange();
            TransformInteractorController.Inst.interActor.Settup(targetTrans, behav.OnTransformChange,behav.Init);
        }
    }

    public void AddRecord(ElementUndoData beginData, ElementUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.UGCClothElementUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        UndoRecordPool.Inst.PushRecord(record);
        UndoRedoAct?.Invoke();
    }

    public void ShowHideAllRayTarget(bool isShow)
    {
        for (int i = 0; i < textList.Count; i++)
        {
            textList[i].self.raycastTarget = isShow;
            textList[i].clickImage.raycastTarget = isShow;
        }
    }

    public void ShowHideAllText(bool isShow)
    {
        for (int i = 0; i < textList.Count; i++)
        {
            textList[i].gameObject.SetActive(isShow);
        }
    }
}
