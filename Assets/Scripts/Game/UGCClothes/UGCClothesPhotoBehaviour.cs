using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UGCClothesPhotoBehaviour : ElementBaseBehaviour
{
    public PhotoData data;
    public RawImage self;
    public bool isLoading = false;
    private Coroutine loadCor;
    public SavePhotoType photoType;
    public Animation loader;
    [HideInInspector] 
    public Transform mirParent;
    public UGCClothesPhotoBehaviour photoDynamicMir;
    public Transform failImage;
    private int loadCont = 0;
    public bool isLoadFail = false;
    public SelectPhotoPanel selectPhotoPanel;
    public BoxCollider selfCollider;
    public Texture defaultImage;

    public override void OnCopy()
    {
        base.OnCopy();
        if (UGCClothesPhotoManager.Inst.IsReachMaximumValue())
        {
            return;
        }
        var copyObj = GameObject.Instantiate(gameObject, MainUGCResPanel.Inst.elementPanel);
        var behav = copyObj.GetComponent<UGCClothesPhotoBehaviour>();
        behav.transform.localPosition += copyOffset;
        behav.photoDynamicMir = UGCClothesPhotoManager.Inst.CreatMirrorText(self, mirParent);
        behav.OnTransformChange();
        behav.photoDynamicMir.gameObject.SetActive(false);
        if (isLoading)
        {
            behav.LoadPhoto();
        }
        else if (!isLoadFail && !string.IsNullOrEmpty(data.photoUrl))
        {
            behav.photoDynamicMir.gameObject.SetActive(true);
        }
        UGCClothesPhotoManager.Inst.AddPhoto(behav);
        if (TransformInteractorController.Inst.interActor)
        {
            TransformInteractorController.Inst.interActor.Settup(behav.self.rectTransform, behav.OnTransformChange,behav.Init);
        }
        UGCClothesPhotoManager.Inst.AddCreateRecord(copyObj, (int)behav.type);
        behav.AddClickEvent();
    }

    public override void SetSelfData()
    {
        data.pos = DataUtils.Vector3ToString(transform.localPosition);
        data.sizeDelta = DataUtils.Vector2ToString(self.rectTransform.sizeDelta);
        data.rot = DataUtils.Vector3ToString(transform.localEulerAngles);
        data.hierarchy = hierarchy;
    }

    public override void Select()
    {
        base.Select();
        selectPhotoPanel.gameObject.SetActive(true);
        selectPhotoPanel.ShowPanel(this);
    }
    #region ����ͼƬ

    public void LoadPhoto()
    {
        if (string.IsNullOrEmpty(data.photoUrl))
        {
            return;
        }
        loader.gameObject.SetActive(true);
        loader.Play();
        isLoading = true;
        isLoadFail = false;
        failImage.gameObject.SetActive(false);
        StopLoadPhoto();
        loadCont++;
        loadCor = CoroutineManager.Inst.StartCoroutine(LoadTexture(data.photoUrl, (tex) =>
        {
            isLoading = false;
            loadCont = 0;
            Texture2D nTex = tex;
            SetTexture(nTex);
            CloseLoader();
            if (photoDynamicMir)
            {
                photoDynamicMir.gameObject.SetActive(true);
            }
        }, () =>
        {
            if (loadCont < 2)
            {
                CoroutineManager.Inst.StopCoroutine(loadCor);
                loadCor = null;
                LoadPhoto();
                return;
            }
            isLoading = false;
            isLoadFail = true;
            loadCont = 0;
            CloseLoader();
            failImage.gameObject.SetActive(true);
            if (photoDynamicMir)
            {
                photoDynamicMir.gameObject.SetActive(false);
            }
        }));
    }

    private IEnumerator LoadTexture(string url, Action<Texture2D> onSuccess, Action onFail)
    {
        yield return new WaitUntil(UGCClothesPhotoManager.Inst.IsCanEnterLoadQueue);
        //������ͼƬ
        UnityWebRequest www = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;
        www.timeout = 15;
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("LoadTextureError" + www.error);
            onFail();
        }
        else
        {
            texDl.texture.Compress(true);
            onSuccess.Invoke(texDl.texture);
        }
        UGCClothesPhotoManager.Inst.LoadingCount--;
        texDl.Dispose();
        www.Dispose();
    }

    private void SetTexture(Texture tex)
    {
        rectTrans.sizeDelta = AutoSize(tex);
        self.texture = tex;
        selfCollider.size = rectTrans.sizeDelta;
        if(photoDynamicMir != null)
        {
            photoDynamicMir.self.texture = tex;
            photoDynamicMir.gameObject.SetActive(true);
        }
        OnTransformChange();
        SetSelfData();
        if(TransformInteractorController.Inst.interActor)
        {
            TransformInteractorController.Inst.GetInterActor().RefreshTransfrom(rectTrans);
        }    
    }

    private Vector2 AutoSize(Texture tex)
    {
        float newRatio = (float)tex.height / tex.width;
        float newH = rectTrans.sizeDelta.x * newRatio;
        return new Vector2(rectTrans.sizeDelta.x, newH);
    }

    private void CloseLoader()
    {
        loader.Stop();
        loader.gameObject.SetActive(false);
    }
#endregion


    public override void OnTransformChange()
    {
        if (photoDynamicMir)
        {
            Copy(self, photoDynamicMir.self);
        }
    }

    private void Copy(RawImage org, RawImage mir)
    {
        mir.rectTransform.localPosition = org.rectTransform.localPosition;
        mir.rectTransform.localScale = org.rectTransform.localScale;
        mir.rectTransform.sizeDelta = org.rectTransform.sizeDelta;
        mir.rectTransform.localEulerAngles = org.rectTransform.localEulerAngles;
        mir.texture = org.texture;
    }

    public override GameObject GetDynamicMir()
    {
        if (photoDynamicMir)
        {
            return photoDynamicMir.gameObject;
        }
        return null;
    }

    public override void OnDis()
    {
        base.OnDis();
        StopLoadPhoto();
        UGCClothesPhotoManager.Inst.RemovePhoto(this);
        if (photoDynamicMir)
        {
            SecondCachePool.Inst.DestroyEntity(photoDynamicMir.gameObject);
        }
        if (UGCClothesPhotoManager.Inst.photoList.Count <= 0)
        {
            MainUGCResPanel.Inst.PaintBtn.isOn = true;
        }
        loader.gameObject.SetActive(false);
    }

    private void StopLoadPhoto()
    {
        if (loadCor != null)
        {
            CoroutineManager.Inst.StopCoroutine(loadCor);
            loadCor = null;
            loadCont = 0;
            if (UGCClothesPhotoManager.Inst.LoadingCount > 0)
            {
                UGCClothesPhotoManager.Inst.LoadingCount--;
            }
        }
    }

    public override void OnClick(GameObject obj)
    {
        base.OnClick(obj);
        if (PaintTool.Current.pType != PaintType.Photo)
        {
            Toggle photoToggle = MainUGCResPanel.Inst.photoBtn;
            photoToggle.SetIsOnWithoutNotify(true);
            PaintTool.Current.SetPaintType(PaintType.Photo);
        }
    }

    public override GameObject ExcuteMirObj()
    {
        if (photoDynamicMir)
        {
            return photoDynamicMir.gameObject;
        }
        return null;
    }

    public override void SetMirSiblingIndex()
    {
        if(photoDynamicMir != null)
        {
            photoDynamicMir.rectTrans.SetSiblingIndex(rectTrans.GetSiblingIndex());
        }
    }

    public override void CreatUndoData(ElementUndoData data)
    {
        base.CreatUndoData(data);
        data.tex = self.texture;
        data.url = this.data.photoUrl;
    }

    public override void RedoInfo()
    {
        UGCClothesPhotoManager.Inst.AddPhoto(this);
    }

    public override void UndoInfo()
    {
        UGCClothesPhotoManager.Inst.RemovePhoto(this);
    }

    public void OnDestroy()
    {
        UGCClothesPhotoManager.Inst.RemovePhoto(this);
    }
}
