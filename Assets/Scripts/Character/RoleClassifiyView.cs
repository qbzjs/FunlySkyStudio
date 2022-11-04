using RedDot;
using AvatarRedDotSystem;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public enum ClassifyType
{
    //编号数据固定，非必要不更改
    none = -1,
    //一级分类
    collections = 1, //混排
    news = 2, //混排
    eyes = 3,
    brows = 4,
    nose = 5,
    mouth = 6,
    blush = 7,
    hair = 8,
    skin = 9,
    outfits = 10,
    headwear = 11,
    glasses = 12,
    shoes = 13,
    accessories = 14,
    bag = 15,
    patterns = 17,
    hand = 18,
    digitalCollect = 19, //混排
    special = 21,
    my = 24, //特殊分类：saves/ rewards/ airdrop
    //二级分类
    saves = 0, //混排
    ugcCloth = 16,
    DCUGCCloth = 20,
    rewards = 23, //混排
    airdrop = 25, //混排
    ugcPatterns = 26,
    effects = 28 //特效
}

public enum BodyPartType
{
    face,
    body,
}

public enum EntryFilterType
{
    none,
    firstEntry, //新用户过滤
}

public enum SpecialEmoHttpReq
{
    sword = 27,
}

[Serializable]
public class BodyPartInfo
{
    public int bodyPart;
    public List<int> classifyList = new List<int>();
}

[Serializable]
public class ClassifyConfigData
{
    public int classifyType;
    public int zoomType; //0:zoomin  1:zoomout
    public int entryFilter; //过滤类型  0:不需要  1:新用户
    public bool hasBG; //专属背景
    public string iconName;
    public string redDotsKind;
}

/// <summary>
/// Author:Meimei-LiMei
/// Description:人物形象分类的UI管理
/// Date: 2022/3/31 11:47:31
/// </summary>
public class RoleClassifiyView : MonoBehaviour
{
    public GameObject roleClassifyItem;
    public Image backgroundImage;
    public Image shadowImg;
    public Image logoImg;
    public Transform[] classifyParents;
    public Transform[] classifyViews;
    public Toggle[] bodyPartTogs;
    public BaseView[] RolePanels;
    public string[] AtlasStrings;
    public static RoleClassifiyView Ins;
    [HideInInspector]
    public List<RoleClassifyItem> roleClassifyItems = new List<RoleClassifyItem>();
    public Dictionary<string, RoleClassifyItem> mClassifyItems = new Dictionary<string, RoleClassifyItem>();//替换上面的list 快速查询  key对应ClassifyType
    private ClassifyType[] curClassifyType;
    private bool[] isFirstEnterView;
    private int curSelectPart;
    private Color defDoneColor = new Color32(43, 43, 43, 255);
    private Image backImg;
    private Text doneTex;
    [HideInInspector]
    public RoleClassifyItem curSelectItem;
    private readonly BodyPartType defSelectPart = BodyPartType.body;
    private List<BodyPartInfo> partConDatas = new List<BodyPartInfo>();
    private List<ClassifyConfigData> classifyConDatas = new List<ClassifyConfigData>();
    private void Awake()
    {
        if (Ins == null)
        {
            Ins = this;
        }

        InitAtlas();
        InitConfigData();
        InitViewState();
    }

    private void InitAtlas()
    {
        for (var i = 0; i < AtlasStrings.Length; i++)
        {
            var str = AtlasStrings[i];
            if (!String.IsNullOrEmpty(str))
            {
                RolePanels[i].sprite = ResManager.Inst.LoadRes<SpriteAtlas>(str);
            }
        }
    }
    
    public Dictionary<ClassifyType, ENodeType> mClassifyType2TreeNodeTypeMapping = new Dictionary<ClassifyType, ENodeType>() {
        {ClassifyType.saves,ENodeType.saves},
        {ClassifyType.collections,ENodeType.collections},
        {ClassifyType.news,ENodeType.news},
        {ClassifyType.eyes,ENodeType.eyes},
        {ClassifyType.brows,ENodeType.brows},
        {ClassifyType.nose,ENodeType.nose},
        {ClassifyType.mouth,ENodeType.mouth},
        {ClassifyType.blush,ENodeType.blush},
        {ClassifyType.hair,ENodeType.hair},
        {ClassifyType.skin,ENodeType.skin},
        {ClassifyType.outfits,ENodeType.outfits},
        {ClassifyType.headwear,ENodeType.headwear},
        {ClassifyType.glasses,ENodeType.glasses},
        {ClassifyType.shoes,ENodeType.shoes},
        {ClassifyType.accessories,ENodeType.accessories},
        {ClassifyType.bag,ENodeType.bag},
        {ClassifyType.ugcCloth,ENodeType.ugcCloth},
        {ClassifyType.patterns,ENodeType.patterns},
        {ClassifyType.hand,ENodeType.hand},
        {ClassifyType.special,ENodeType.special},
        {ClassifyType.digitalCollect,ENodeType.digitalCollect},
        {ClassifyType.my,ENodeType.my},
        {ClassifyType.airdrop,ENodeType.airdrop},
        {ClassifyType.effects,ENodeType.effect},
    };

    private void InitConfigData()
    {
        partConDatas = ResManager.Inst.LoadJsonRes<List<BodyPartInfo>>("Configs/RoleData/BodyPartsConfig");
        classifyConDatas = ResManager.Inst.LoadJsonRes<List<ClassifyConfigData>>("Configs/RoleData/ClassifyIconConfig");
    }
    private void InitViewState()
    {
        isFirstEnterView = new bool[bodyPartTogs.Length];
        curClassifyType = new ClassifyType[bodyPartTogs.Length];
        for (int i = 0; i < bodyPartTogs.Length; i++)
        {
            isFirstEnterView[i] = true;
        }
    }
    public void Start()
    {
        ToggleInit();
        InitCompData();
    }
    private void InitCompData()
    {
        backImg = RoleUIManager.Inst.BtnRoleReturn.GetComponent<Image>();
        doneTex = RoleUIManager.Inst.BtnConfirmAvatar.GetComponentInChildren<Text>();
        ChangeBGToDefault();
    }
    public void ToggleInit()
    {
        for (int i = 0; i < bodyPartTogs.Length; i++)
        {
            int bodyType = i;
            bodyPartTogs[i].onValueChanged.AddListener(x => OnToggleSelect(x, bodyType));
        }
    }
    public void OnToggleSelect(bool isOn, int bodyPartType)
    {
        if (!isOn)
        {
            return;
        }
        foreach (var data in partConDatas)
        {
            classifyViews[data.bodyPart].gameObject.SetActive(data.bodyPart == bodyPartType);
        }
        curSelectPart = bodyPartType;

        if (isFirstEnterView[bodyPartType])
        {
            var scrollRect = classifyViews[bodyPartType].GetComponent<ScrollRect>();
            scrollRect.horizontalNormalizedPosition = 0f;
            isFirstEnterView[bodyPartType] = false;
            //处理特殊情况
            if (bodyPartType == (int)BodyPartType.body)
            {
                switch ((ROLE_TYPE)GameManager.Inst.engineEntry.subType)
                {

                    case ROLE_TYPE.SET_REWARDS:
                        SetClassifyItemSelect(BodyPartType.body, ClassifyType.my);
                        return;
                    case ROLE_TYPE.SET_IMAGE:
                        SetClassifyItemSelect(BodyPartType.body, ClassifyType.news);
                        return;
                    case ROLE_TYPE.FIRST_ENTRY:
                        SetClassifyItemSelect(BodyPartType.body, ClassifyType.hair);
                        return;
                }
            }
            if (bodyPartType == (int)BodyPartType.face && (ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
            {
                SetClassifyItemSelect(BodyPartType.face, ClassifyType.eyes);
                return;
            }
            SetClassifyItemSelect((BodyPartType)bodyPartType, 0);
        }
        else
        {
            SetClassifyItemSelect((BodyPartType)bodyPartType, curClassifyType[bodyPartType]);
        }
    }
    public void InitClassifyView()
    {
        for (int i = 0; i < RolePanels.Length; i++)
        {
            RolePanels[i].gameObject.SetActive(false);
        }
        int faceIndex = (int)BodyPartType.face;
        var horizontalLayoutGroup = classifyParents[faceIndex].GetComponent<HorizontalLayoutGroup>();
        if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            horizontalLayoutGroup.spacing = 90f;
        }
        else
        {
            horizontalLayoutGroup.spacing = 24.5f;
        }
        foreach (var data in partConDatas)
        {
            SetClassifyView(data);
        }
        SelectDefaultToggle();
    }
    private void SelectDefaultToggle()
    {
        int defIndex = (int)defSelectPart;
        bodyPartTogs[defIndex].isOn = true;
        OnToggleSelect(true, defIndex);
    }
    private void SetClassifyView(BodyPartInfo partInfo)
    {
        for (var i = 0; i < partInfo.classifyList.Count; i++)
        {
            var classifyData = classifyConDatas.Find(x => x.classifyType == partInfo.classifyList[i]);
            //新用户
            if ((ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
            {
                if (classifyData.entryFilter == (int)EntryFilterType.firstEntry)
                {
                    continue;
                }
            }
            var classifyItem = Instantiate(roleClassifyItem, classifyParents[partInfo.bodyPart]);
            var recTran = classifyItem.GetComponent<RectTransform>();
            var itemScript = classifyItem.GetComponent<RoleClassifyItem>();
            itemScript.Atlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/ClassifyIcon");
            itemScript.SetClassifyName(i, classifyData.iconName, SetClassifySelect, (ViewType)classifyData.zoomType, (ClassifyType)classifyData.classifyType, (BodyPartType)partInfo.bodyPart);
            roleClassifyItems.Add(itemScript);
            string key= ((ClassifyType)classifyData.classifyType).ToString();
            if (!mClassifyItems.ContainsKey(key))
            {
                mClassifyItems.Add(key, itemScript);
            }
        }
    }
    private void SetClassifySelect(ClassifyType classifyType)
    {
        SetLogoImgState(false);
        curClassifyType[curSelectPart] = classifyType;
        for (int i = 0; i < RolePanels.Length; i++)
        {
            RolePanels[i].gameObject.SetActive(RolePanels[i].classifyType == classifyType);
            if (RolePanels[i].classifyType == classifyType)
            {
                RolePanels[i].OnSelect();
            }
        }
        var data = classifyConDatas.Find(x => x.classifyType == (int)classifyType);
        if (!data.hasBG)
        {
            ChangeBGToDefault();
        }
        AdjustViewManager.Inst.CloseCurrentAdjustView(null);
    }
    public void ChangeBGToDefault()
    {
        backImg.sprite = SpriteAtlasManager.Inst.GetAvatarCommonSprite("ic_back");
        doneTex.color = defDoneColor;
        RoleConfigDataManager.Inst.SetAvatarIconDynamic(backgroundImage, "bg2", SpriteAtlasManager.Inst.AvatarCommonAtlas);
        RoleConfigDataManager.Inst.SetAvatarIconDynamic(shadowImg, "def_shadow", SpriteAtlasManager.Inst.AvatarCommonAtlas);
    }
    public void SetLogoImgState(bool isAct)
    {
        logoImg.gameObject.SetActive(isAct);
    }
    public void ChangeBGIconColorToWhite()
    {
        this.backImg.sprite = SpriteAtlasManager.Inst.GetAvatarCommonSprite("ic_back_w");
        this.doneTex.color = DataUtils.DeSerializeColorByHex("#FFFFFF");
    }
    private void SetClassifyItemSelect(BodyPartType partType, ClassifyType type)
    {
        roleClassifyItems.ForEach(x =>
        {
            if (x.bodyPartType == partType && x.classifyType == type)
            {
                x.OnSelectClick();
            }
        });
    }
    private void SetClassifyItemSelect(BodyPartType partType, int index)
    {
        roleClassifyItems.ForEach(x =>
        {
            if (x.bodyPartType == partType && x.classifyIndex == index)
            {
                x.OnSelectClick();
            }
        });
    }
    //不适用于混排面板
    public void SetClassifyItemSelect(ClassifyType type, int id)
    {
        roleClassifyItems.ForEach(x =>
        {
            if (x.classifyType == type)
            {
                OnToggleSelect(true, (int)x.bodyPartType);
                BaseView bView = GetViewByType(type);
                if (bView) bView.OnSelectItem(id, null);
                x.OnSelectClick();
            }
        });
    }
    /// <summary>
    /// 重置当前界面的选中 item
    /// </summary>
    public void ResetCurViewItemSelect()
    {
        for (int i = 0; i < RolePanels.Length; i++)
        {
            if (RolePanels[i].classifyType == curClassifyType[curSelectPart])
            {
                RolePanels[i].UpdateSelectState();
            }
        }
        AdjustViewManager.Inst.CloseCurrentAdjustView(() =>
        {
            if (curClassifyType[curSelectPart] == ClassifyType.news || curClassifyType[curSelectPart] == ClassifyType.collections)
            {
                // 合集界面需要调用自定义方法关闭 Adjust 界面
                AdjustViewManager.Inst.CloseAdjustView();
            }
        });
    }
    //根据ClassifyType找BaseView面板(注意判空), 47版本兼容ugcType查找View
    public BaseView GetViewByType(ClassifyType type)
    {
        var pgctype = RoleConfigDataManager.Inst.GetPGCTypeByUGCType(type);
        if (pgctype != ClassifyType.none)
        {
            type = pgctype;
        }
        return Array.Find(RolePanels, v => v.classifyType == type);
    }
    public ViewType GetZoomType(ClassifyType type)
    {
        int index = classifyConDatas.FindIndex(x => x.classifyType == (int)type);
        if (index < 0)
        {
            return ViewType.ZoomWholeBody;
        }
        return (ViewType)classifyConDatas[index].zoomType;
    }
    public void RedDotInited(List<AvatarRedDots> datas)
    {
        //初始化一级红点
        RedDotTree tree= RoleMenuView.Ins.mAvatarRedDotManager.Tree;
        tree.AddRedDot(bodyPartTogs[0].gameObject,(int)ENodeType.Root,(int)ENodeType.Face,ERedDotPrefabType.Type2);
        tree.AddRedDot(bodyPartTogs[1].gameObject,(int)ENodeType.Root,(int)ENodeType.Body,ERedDotPrefabType.Type2);
        //初始化二级红点
        int len = datas.Count;
        for (int i = 0; i < len; i++)
        {
            AvatarRedDots data = datas[i];
            string strId = data.resourceKind;
            RoleClassifyItem item = null;
            if (mClassifyItems.TryGetValue(strId,out item))
            {
                item.AttachRedDot(tree);
            }
        }
        //view界面的处理，三级红点 ugc下的 marketplace和dc
        len = RolePanels.Length;
        for (int i = 0; i < len; i++)
        {
            BaseView baseView = RolePanels[i];
            RoleClassifyItem item = GetClassifyItem(baseView.classifyType);
            if (item != null) baseView.InitRedDot(item, datas, tree);

        }
    }
    private RoleClassifyItem GetClassifyItem(ClassifyType classifyType)
    {
        RoleClassifyItem item = null;
        if (mClassifyItems.TryGetValue(classifyType.ToString(), out item))
        {
            return item;
        }
        return null;
    }
    private void OnDestroy()
    {
        Ins = null;
    }
}
