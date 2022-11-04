using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using Newtonsoft.Json;

public class RoleUgcManager : BMonoBehaviour<RoleUgcManager>
{
    private HttpReqQuerry httpRequest = new HttpReqQuerry();
    private int pageSize = 32;


    private void Start()
    {
#if !UNITY_EDITOR
        MobileInterface.Instance.AddClientRespose(MobileInterface.updateWearClothsResource, OnUpdateWearClothsResource);
        MobileInterface.Instance.AddClientRespose(MobileInterface.updateClothList, OnUpdateClothList);

#endif
    }
    #region Get-Wear
    /// <summary>
    /// 端上点击Wear回调事件
    /// </summary>
    /// <param name="content"></param>
    public void OnUpdateWearClothsResource(string content)
    {
        WearClothInfo clothInfo = JsonUtility.FromJson<WearClothInfo>(content);
        UGCClothInfo ugcClothInfo = new UGCClothInfo
        {
            mapId = clothInfo.clothMapId,
            clothesJson = clothInfo.clothesJson,
            clothesUrl = clothInfo.clothesUrl,
            templateId = clothInfo.templateId,
            dataSubType = clothInfo.dataSubType
        };
        WearUgc(ugcClothInfo);
    }
    /// <summary>
    /// 试穿UGC衣服、面部彩绘
    /// </summary>
    /// <param name="type"></param>
    /// <param name="ugcClothInfo"></param>
    /// <param name="ugcClothType"></param>
    public void WearUgc(UGCClothInfo ugcClothInfo, UGCClothesResType ugcClothType = UGCClothesResType.UGC)
    {
        switch (ugcClothInfo.dataSubType)
        {
            case (int)DataSubType.Clothes:
                RoleClassifiyView.Ins.SetClassifyItemSelect(ClassifyType.outfits, -1);
                WearUgcClothes(ugcClothInfo, ugcClothType);
                break;
            case (int)DataSubType.Patterns:
                RoleClassifiyView.Ins.SetClassifyItemSelect(ClassifyType.patterns, -1);
                WearUgcPatterns(ugcClothInfo, ugcClothType);
                break;
        }
    }
    public void WearUgcClothes(UGCClothInfo ugcClothInfo, UGCClothesResType ugcClothType)
    {
        ClothStyleData clothesData = RoleConfigDataManager.Inst.GetClothesByTemplateId(ugcClothInfo.templateId);
        clothesData.clothesJson = ugcClothInfo.clothesJson;
        clothesData.clothesUrl = ugcClothInfo.clothesUrl;
        clothesData.clothMapId = ugcClothInfo.mapId;
        var curRoleData = RoleMenuView.Ins.roleData;
        curRoleData.cloId = clothesData.id;
        curRoleData.clothesJson = ugcClothInfo.clothesJson;
        curRoleData.clothesUrl = ugcClothInfo.clothesUrl;
        curRoleData.clothMapId = ugcClothInfo.mapId;
        curRoleData.ugcClothType = (int)ugcClothType;
        var roleComp = RoleMenuView.Ins.rController;
        ClothLoadManager.Inst.LoadUGCClothRes(clothesData, roleComp);
        if (RoleMenuView.Ins != null)
        {
            var outfitsView = RoleMenuView.Ins.GetView<OutfitsView>();
            outfitsView.SetSelectTog(ugcClothType == UGCClothesResType.DC ? 2 : 1);
            var iconView = outfitsView.GetComponentInChildren<RoleClothStyleView>(true);
            if (iconView)
            {
                iconView.OnSelectItemByID(curRoleData.clothMapId, curRoleData.cloId);
            }
        }
        DataLogUtils.AVatarUGCWear(ugcClothInfo.mapId, (int)RoleResGrading.Normal, ClassifyType.ugcCloth);//DC不可wear
    }
    public void WearUgcPatterns(UGCClothInfo ugcClothInfo, UGCClothesResType ugcPatternType)
    {
        PatternStyleData patternData = RoleConfigDataManager.Inst.GetPatternByTemplateId(ugcClothInfo.templateId);
        patternData.patternJson = ugcClothInfo.clothesJson;
        patternData.patternUrl = ugcClothInfo.clothesUrl;
        patternData.patternMapId = ugcClothInfo.mapId;
        var curRoleData = RoleMenuView.Ins.roleData;
        curRoleData.fpId = patternData.id;
        curRoleData.ugcFPData = new UgcResData
        {
            ugcMapId = ugcClothInfo.mapId,
            ugcJson = ugcClothInfo.clothesJson,
            ugcUrl = ugcClothInfo.clothesUrl,
            ugcType = (int)ugcPatternType
        };
        var roleComp = RoleMenuView.Ins.rController;
        roleComp.SetUgcPatternStyle(patternData);
        if (RoleMenuView.Ins != null)
        {
            var patternView = RoleMenuView.Ins.GetView<PatternView>();
            patternView.SetSelectTog(ugcPatternType == UGCClothesResType.DC ? 2 : 1);
            var iconView = patternView.GetComponentInChildren<RoleUgcPatternView>(true);
            if (iconView)
            {
                iconView.OnSelectItemByID(ugcClothInfo.mapId, curRoleData.fpId);
            }
            patternView.SetAdjustView2Normal(patternView.adjustView, patternData);
        }
        DataLogUtils.AVatarUGCWear(ugcClothInfo.mapId, (int)RoleResGrading.Normal, ClassifyType.ugcPatterns);//DC不可wear
    }
    #endregion

    private void OnUpdateClothList(string content)
    {
        var patternView = RoleMenuView.Ins.GetView<PatternView>();
        var patternUgcView = patternView.GetComponentInChildren<RoleUgcPatternView>(true);
        if (patternUgcView)
        {
            patternUgcView.OnUpdateClothList(content);
        }
        var outfitsView = RoleMenuView.Ins.GetView<OutfitsView>();
        var outfitsUgcView = outfitsView.GetComponentInChildren<RoleClothStyleView>(true);
        if (outfitsUgcView)
        {
            outfitsUgcView.OnUpdateClothList(content);
        }
    }
}
