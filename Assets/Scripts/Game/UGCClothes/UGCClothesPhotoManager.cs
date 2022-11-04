using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UGCClothesPhotoManager : CInstance<UGCClothesPhotoManager>
{
    public RawImage photoPrefabs;
    public UGCClothesPhotoBehaviour targetBehav;
    public List<UGCClothesPhotoBehaviour> photoList = new List<UGCClothesPhotoBehaviour>();
    public float textScalingRatio = 1.5f;
    public float minSize = 370f;
    private int maxNum = 10;
    public int LoadingQueue = 1; //下载队列长度
    public int LoadingCount; //当前下载数
    private Action UndoRedoAct;
    public void Init()
    {
        photoPrefabs = MainUGCResPanel.Inst.photoPrefabs;
        UndoRedoAct = MainUGCResPanel.Inst.UpdateUndoBtnView;
    }

    public void AddPhoto(UGCClothesPhotoBehaviour behav)
    {
        if (!photoList.Contains(behav))
        {
            photoList.Add(behav);
        }
    }

    public void ShowPhotoByIndex(int part)
    {
        for (var i = 0; i < photoList.Count; i++)
        {
            photoList[i].gameObject.SetActive(photoList[i].part == part);
        }
    }
    public void RemovePhoto(UGCClothesPhotoBehaviour behav)
    {
        if (photoList.Contains(behav))
        {
            photoList.Remove(behav);
        }
    }

    public bool IsLoadingOrFail()
    {
        for (int i = 0; i < photoList.Count; i++)
        {
            if (photoList[i].isLoadFail || photoList[i].isLoading)
            {
                return true;
            }
        }
        return false;
    }
    public UGCClothesPhotoBehaviour CreatPhoto(int ClothesIndex, PhotoData pData, Transform par,SelectPhotoPanel selectPanel)
    {
        if (IsReachMaximumValue())
        {
            return null;
        }
        var photoGo = GameObject.Instantiate(photoPrefabs, MainUGCResPanel.Inst.elementPanel);
        var behav = photoGo.gameObject.AddComponent<UGCClothesPhotoBehaviour>();
        var collider = photoGo.gameObject.AddComponent<BoxCollider>();
        SetBehav(behav, photoGo, ClothesIndex);
        SetData(pData, behav);
        collider.size = new Vector3(photoGo.rectTransform.sizeDelta.x, photoGo.rectTransform.sizeDelta.y, 0);
        targetBehav = behav;
        behav.mirParent = par;
        behav.gameObject.SetActive(true);
        AddPhoto(behav);
        behav.selectPhotoPanel = selectPanel;
        behav.photoDynamicMir = CreatMirrorText(photoGo, par);
        behav.photoDynamicMir.gameObject.SetActive(false);
        behav.OnTransformChange();
        return behav;
    }

    public void SetData(PhotoData pData, UGCClothesPhotoBehaviour behav)
    {
        var textGo = behav.self;
        if (pData != null)
        {
            behav.data = pData;
            textGo.rectTransform.localPosition = DataUtils.DeSerializeVector3(behav.data.pos);
            textGo.rectTransform.sizeDelta = DataUtils.DeSerializeVector2(behav.data.sizeDelta);
            textGo.rectTransform.localEulerAngles = DataUtils.DeSerializeVector3(behav.data.rot);
            textGo.rectTransform.localScale = Vector3.one;
            behav.hierarchy = behav.data.hierarchy;
            behav.LoadPhoto();
            textGo.gameObject.SetActive(true);
        }
        else
        {
            textGo.rectTransform.localPosition = Vector2.zero;
            textGo.rectTransform.localScale = Vector3.one;
            textGo.rectTransform.sizeDelta = new Vector2(behav.minSize, behav.minSize / behav.scalingRatio);
            textGo.rectTransform.localEulerAngles = Vector3.zero;
            behav.hierarchy = behav.data.hierarchy;
            textGo.gameObject.SetActive(true);
        }
        behav.SetSelfData();
    }

    public void SetBehav(UGCClothesPhotoBehaviour behav,RawImage photoGo, int ClothesIndex)
    {
        behav.self = photoGo;
        behav.part = ClothesIndex;
        behav.rectTrans = photoGo.rectTransform;
        behav.type = BehaviourType.Photo;
        behav.data = new PhotoData();
        behav.loader = behav.gameObject.GetComponentInChildren<Animation>();
        behav.loader.gameObject.SetActive(false);
        behav.failImage = behav.transform.Find("loadFail");
        behav.failImage.gameObject.SetActive(false);
        behav.minSize = minSize;
        behav.scalingRatio = textScalingRatio;
        behav.selfCollider = photoGo.gameObject.GetComponent<BoxCollider>();
        behav.defaultImage = photoGo.texture;
        behav.AddClickEvent();
    }

    public UGCClothesPhotoBehaviour CreatMirrorText(RawImage photoGo,Transform par)
    {
        var mirrorPhoto = GameObject.Instantiate(photoGo, par);
        mirrorPhoto.rectTransform.localPosition = photoGo.rectTransform.localPosition;
        mirrorPhoto.rectTransform.localScale = photoGo.rectTransform.localScale;
        mirrorPhoto.rectTransform.sizeDelta = photoGo.rectTransform.sizeDelta;
        mirrorPhoto.rectTransform.localEulerAngles = photoGo.rectTransform.localEulerAngles;
        //mirrorPhoto.gameObject.SetActive(true);
        var behav = mirrorPhoto.GetComponent<UGCClothesPhotoBehaviour>();
        behav.loader.gameObject.SetActive(false);
        return behav;
    }

    public void OnChangePart(int curPart)
    {
        for (int i = 0; i < photoList.Count; i++)
        {
            if (photoList[i].part == curPart)
            {
                photoList[i].gameObject.SetActive(true);
            }
            else
            {
                photoList[i].gameObject.SetActive(false);
            }
        }
    }

    public List<PhotoData> GetPartPhotoDatas(int part)
    {
        List<PhotoData> pDatas = new List<PhotoData>();
        for (int i = 0; i < photoList.Count; i++)
        {
            if (photoList[i].part == part)
            {
                photoList[i].SetSelfData();
                pDatas.Add(photoList[i].data);
            }
        }
        return pDatas;
    }

    public bool IsReachMaximumValue()
    {
        if (photoList.Count >= maxNum)
        {
            TipPanel.ShowToast("Oops! Only 10 photos can be added.");
            return true;
        }
        return false;
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

    public void ShowHideAllRayTarget(bool isShow)
    {
        for (int i = 0; i < photoList.Count; i++)
        {
            photoList[i].self.raycastTarget = isShow;
            photoList[i].clickImage.raycastTarget = isShow;
        }
    }

    public bool IsCanEnterLoadQueue()
    {
        if (LoadingCount < LoadingQueue)
        {
            LoadingCount++;
            return true;
        }
        return false;
    }

    public string[] GetUrlArr()
    {
        List<string> urlList = new List<string>();
        for (int i = 0; i < photoList.Count; i++)
        {
            if (!string.IsNullOrEmpty(photoList[i].data.photoUrl))
            {
                urlList.Add(photoList[i].data.photoUrl);
            }
        }
        return urlList.ToArray();
    }

    public void SetTextureUndo(RectTransform rect,Texture tex,string url)
    {
        var behav = rect.GetComponent<UGCClothesPhotoBehaviour>();
        if (tex != null && behav != null)
        {
            behav.self.texture = tex;
            behav.data.photoUrl = url;
            if (behav.photoDynamicMir)
            {
                behav.photoDynamicMir.self.texture = tex;
            }
            bool isShow = behav.photoDynamicMir.self.texture != behav.photoDynamicMir.defaultImage ? true : false;
            behav.photoDynamicMir.gameObject.SetActive(isShow);
        }
    }

    public void ShowHideAllPhoto(bool isShow)
    {
        for (int i = 0; i < photoList.Count; i++)
        {
            photoList[i].gameObject.SetActive(isShow);
        }
    }
}
