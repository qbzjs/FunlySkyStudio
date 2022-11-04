/// <summary>
/// Author:Mingo-LiZongMing
/// Description:
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class PublicUGCClothInfo
{
    public string clothesJson;
    public string clothesUrl;
    public string mapId;
    public int templateId;
    public PublicPGCInfo dcPgcInfo;
    public int dataSubType;
}

[Serializable]
public class PublicPGCInfo
{
    public int classifyType;
    public int pgcId;
}

public class TryOnPanel : BasePanel<TryOnPanel>
{
    private const int ITEM_DESC_MAX_LINES = 3;
    private const string MORE_SUFFIX = "<color=#B5ABFF>... More</color> \n \n";
    private const string HIDE_SUFFIX = "<color=#B5ABFF>... Hide</color> \n \n";

    public Button _btnBack;
    public GameObject _pgcObj;
    public Text _pgcTxt;
    public GameObject _dcObj;
    public Text _dcTxt;
    public RawImage _itemImg;
    public Text _itemNameTxt;
    public Text _itemDescTxt;
    public Button _itemDescBtn;
    

    private string _itemName = "";
    private string _itemDesc = "";
    private MapInfo _data;

    private UGCClothesPreviewHandle _handle;

    private Action _returnAct;
    private RoleController _rController;
    private void InitUIData()
    {
        _btnBack.onClick.AddListener(() => {
            _returnAct?.Invoke();
            cookie?.Stop();
        });
    }

    private void InitHandleData()
    {
        _handle = this.GetComponentInChildren<UGCClothesPreviewHandle>();
        _handle.rController = _rController;
    }

    private void Redraw()
    {
        if(_data.isPGC > 0)
        {
            _pgcObj.SetActive(true);
            _dcObj.SetActive(false);
        }
        else if(_data.isDC > 0)
        {
            _pgcObj.SetActive(false);
            _dcObj.SetActive(true);
        }

        if (_data.isPGC > 0 && !string.IsNullOrEmpty(_data.bannerName))
        {
            LocalizationConManager.Inst.SetLocalizedContent(_pgcTxt, _data.bannerName);
        }

        if (_data.isDC > 0 && _data.dcInfo != null)
        {
            StartCoroutine(GameUtils.LoadTexture(_data.dcInfo.itemCover, (texture) => {
                if (_itemImg != null)
                    _itemImg.texture = texture;
            }, (error) => {
                LoggerUtils.LogError(error);
            }));
        }
        else
        {
            StartCoroutine(GameUtils.LoadTexture(_data.mapCover, (texture) => {
                if (_itemImg != null)
                    _itemImg.texture = texture;
            }, (error) => {
                LoggerUtils.LogError(error);
            }));
        }

        LocalizationConManager.Inst.SetSystemTextFont(_itemNameTxt);
        LocalizationConManager.Inst.SetSystemTextFont(_itemDescTxt);

        _itemNameTxt.text = _itemName;
        _itemDescTxt.text = _itemDesc;
        Canvas.ForceUpdateCanvases();

        if (!string.IsNullOrEmpty(_itemDesc))
        {
            var lines = _itemDescTxt.cachedTextGenerator.lineCount;
            if (lines > ITEM_DESC_MAX_LINES)
                ExpandItemDesc();
        }
    }

    private void ExpandItemDesc()
    {
        _itemDescTxt.text = _itemDesc + HIDE_SUFFIX;
        _itemDescBtn.onClick.RemoveAllListeners();
        _itemDescBtn.onClick.AddListener(CollapseItemDesc);
    }

    private void CollapseItemDesc()
    {
        int endIndex = 0;
        for (int i = 0; i < _itemDescTxt.cachedTextGenerator.lineCount; i++)
        {
            if (i == ITEM_DESC_MAX_LINES - 1)
            {
                endIndex = (i == _itemDescTxt.cachedTextGenerator.lines.Count - 1) ? _itemDescTxt.text.Length
                    : _itemDescTxt.cachedTextGenerator.lines[i + 1].startCharIdx;
            }
        }

        _itemDescTxt.text = _itemDesc.Substring(0, endIndex) + MORE_SUFFIX;
        _itemDescBtn.onClick.RemoveAllListeners();
        _itemDescBtn.onClick.AddListener(ExpandItemDesc);
    }

    public void SetTryOnReturnAction(Action action)
    {
        _returnAct = action;
    }

    public void SetTryOnData(RoleController roleCtr, MapInfo mapInfo, PublicUGCClothInfo ugcClothInfo)
    {
        this._rController = roleCtr;
        this._data = mapInfo;
        if (_data == null)
        {
            LoggerUtils.Log("Try on. data is null");
            return;
        }
        _itemName = DataUtils.FilterNonStandardText(_data.mapName);
        _itemDesc = DataUtils.FilterNonStandardText(_data.mapDesc);
        Redraw();
        InitUIData();
        InitHandleData();

        var roleData = JsonConvert.DeserializeObject<RoleData>(GameManager.Inst.ugcUserInfo.imageJson);
        bool isPgcData = (ugcClothInfo.dcPgcInfo != null) && ugcClothInfo.dcPgcInfo.classifyType != 0 && ugcClothInfo.dcPgcInfo.pgcId != 0;
        if (isPgcData)
        {
            //DC-PGC流程 || 1.42版本新增签到奖励PGC
            HandleDCRoleData(ugcClothInfo, roleData);
        }
        else
        {
            //ugcCloth流程
            if (ugcClothInfo.dataSubType == (int)DataSubType.Clothes)
            {
                ClothStyleData clothesData = RoleConfigDataManager.Inst.GetClothesByTemplateId(ugcClothInfo.templateId);
                if (clothesData != null)
                {
                    roleData.clothMapId = ugcClothInfo.mapId;
                    roleData.clothesUrl = ugcClothInfo.clothesUrl;
                    roleData.clothesJson = ugcClothInfo.clothesJson;
                    roleData.cloId = clothesData.id;
                    DataLogUtils.LogTryOnWear(ugcClothInfo.mapId, mapInfo.isDC, ClassifyType.ugcCloth);
                }
            }
            if (ugcClothInfo.dataSubType == (int)DataSubType.Patterns)
            {
                PatternStyleData patternDatd = RoleConfigDataManager.Inst.GetPatternByTemplateId(ugcClothInfo.templateId);
                if (patternDatd != null)
                {
                    roleData.ugcFPData = new UgcResData
                    {
                        ugcMapId = ugcClothInfo.mapId,
                        ugcJson = ugcClothInfo.clothesJson,
                        ugcUrl = ugcClothInfo.clothesUrl,
                    };
                    roleData.fpId = patternDatd.id;
                    //TryOn时恢复默认位置
                    roleData.fpP = patternDatd.pDef;
                    roleData.fpS = patternDatd.sDef;
                    DataLogUtils.LogTryOnWear(ugcClothInfo.mapId, mapInfo.isDC, ClassifyType.ugcPatterns);
                }
            }
        }
        roleCtr.InitRoleByData(roleData);
        //特殊处理一下眼睛
        string eyeName = roleCtr.GetAnimName(roleData.eId);
        roleCtr.SetEyesStyle(eyeName);
        roleCtr.SetSpecialEyesStyle(eyeName);
        roleCtr.SetEyePupilColor(eyeName, roleData.eCr);
        roleCtr.StartEyeAnimation(roleData.eId);
        if(ugcClothInfo.dcPgcInfo != null)
        {
            ClassifyType cType = (ClassifyType)ugcClothInfo.dcPgcInfo.classifyType;
            PlayTryOnSwordAnim(ugcClothInfo.dcPgcInfo.pgcId, (int)cType, roleCtr);
        }
    }

    private void HandleDCRoleData(PublicUGCClothInfo ugcClothInfo, RoleData roleData)
    {
        ClassifyType cType = (ClassifyType)ugcClothInfo.dcPgcInfo.classifyType;
        int id = ugcClothInfo.dcPgcInfo.pgcId;
        if (id == 0 || cType == 0)
        {
            LoggerUtils.LogError("TryOnPanel HandleDCRoleData Failed --> ugcClothInfo data error");
        }
        switch (cType)
        {
            case ClassifyType.headwear:
                var hatData = RoleConfigDataManager.Inst.GetHatStyleDataById(id);
                roleData.hatId = id;
                roleData.hatP = hatData.pDef;
                roleData.hatS = hatData.sDef;
                roleData.hatR = hatData.rDef;
                break;
            case ClassifyType.glasses:
                var glData = RoleConfigDataManager.Inst.GetGlassesStyleDataById(id);
                roleData.glId = id;
                roleData.glP = glData.pDef;
                roleData.glS = glData.sDef;
                roleData.glR = glData.rDef;
                break;
            case ClassifyType.outfits:
                roleData.cloId = id;
                break;
            case ClassifyType.shoes:
                roleData.shoeId = id;
                break;
            case ClassifyType.hand:
                var handData = RoleConfigDataManager.Inst.GetHandStyleDataById(id);
                roleData.hdId = id;
                roleData.hdS = handData.sDef;
                roleData.hdP = handData.pDef;
                roleData.hdR = handData.rDef;
                break;
            case ClassifyType.bag:
                var bagData = RoleConfigDataManager.Inst.GetBagStylesDataById(id);
                roleData.bagId = id;
                roleData.bagS = bagData.sDef;
                roleData.bagP = bagData.pDef;
                roleData.bagR = bagData.rDef;
                break;
            case ClassifyType.eyes:
                var eyesData = RoleConfigDataManager.Inst.GetEyeStyleDataById(id);
                roleData.eId = id;
                roleData.eS = eyesData.sDef;
                roleData.eP = eyesData.pDef;
                roleData.eR = eyesData.rDef;
                break;
            case ClassifyType.effects:
                var effData = RoleConfigDataManager.Inst.GetEffectStyleDataById(id);
                roleData.effId = id;
                roleData.effS = effData.sDef;
                roleData.effP = effData.pDef;
                roleData.effR = effData.rDef;
                break;
        }
        DataLogUtils.AvatarPGCWear(cType, id);
    }

    #region tryOn逻辑
    private Coroutine stopTryOnCor;
    private string selfUid;
    private SoundCookie cookie;
    public void PlayTryOnSwordAnim(int swordId, int part, RoleController roleCom)
    {
        selfUid = GameManager.Inst.ugcUserInfo.uid;
        var sManager = SwordManager.Inst;
        sManager.InitAssetPath();
        var data = sManager.GetRoleIconDataByList(swordId, (BundlePart)part);
        if (data == null)
        {
            return;
        }
        if(data.specialType < 1)
        {
            return;
        }
        SwordAnimDataManager.Inst.InitAnim();
        roleCom.SetBoneActive(false);
        var emoteId = SwordAnimDataManager.Inst.FindemoteId(swordId, (BundlePart)part);
        var emoData = MoveClipInfo.GetAnimName(emoteId);
        string path = "Prefabs/Emotion/Express/" + emoData.name;
        GameObject preFbx = ResManager.Inst.LoadCharacterRes<GameObject>(path);
        if (preFbx == null)
        {
            return;
        }
        var sword = GameObject.Instantiate(preFbx, roleCom.transform);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localEulerAngles = Vector3.zero;
        sword.transform.localScale = Vector3.one;
        roleCom.animCom.Play(data.modelName);
        cookie?.Stop();
        cookie = AKSoundManager.Inst.playEmoSound(emoData.name, gameObject, true, 1);
        var time = GetLengthByName(data.modelName, roleCom.animCom);
        if (stopTryOnCor != null)
        {
            StopTryOnAnim(0, sword, roleCom);
            stopTryOnCor = null;
        }
        stopTryOnCor = CoroutineManager.Inst.StartCoroutine(StopTryOnAnim(time, sword, roleCom));
    }

    public IEnumerator StopTryOnAnim(float time, GameObject obj, RoleController roleCom)
    {
        yield return new WaitForSeconds(time);
        GameObject.Destroy(obj);
        roleCom.animCom.Play("interface_idle");
        roleCom.SetBoneActive(true);
    }

    public float GetLengthByName(string name, Animator anim)
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Equals(name))
            {
                return clip.length;
            }
        }
        return 0;
    }
    #endregion
}
