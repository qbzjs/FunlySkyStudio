using System;
using System.Collections.Generic;
using BasePrimitiveRedDotSystem;
using RedDot;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
public class BasePrimitivePanel : BasePanel<BasePrimitivePanel>
{
    public class PrimitiveItem {

        public GameObject mGameObject;
        public VNode mVNode;
        public int mId;
        public int mIndex;
        public Action<int> mOnClickFunc;
        public PrimitiveItemType type;
        public PrimitiveItem(int index, int id, GameObject gameObject)
        {
            mId = id;
            mGameObject = gameObject;
            mIndex = index;
        }
        public void Init()
        {
            Button btn = mGameObject.transform.GetChild(0).GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
        }
        public void OnClick()
        {
            mOnClickFunc?.Invoke(mIndex);
            if (mVNode != null && mVNode.mLogic.Count > 0)
            {
                int oldValue = mVNode.mLogic.Count;
                mVNode.mLogic.ChangeCount(oldValue - 1);
                RequestCleanRedDot();
            }

        }
        private void RequestCleanRedDot()
        {
            BasePrimitivePanel.Instance.mPrimitiveRedDotSystemManager.RequestCleanRedDot(mId);
        }
        public void AttachRedDotNode()
        {
            mVNode= InternalAttachRedDotNode();
            int old= mVNode.mLogic.Count;
            mVNode.mLogic.ChangeCount(old+1);
        }
        private VNode InternalAttachRedDotNode()
        {
            RedDotTree tree = BasePrimitivePanel.Instance.mPrimitiveRedDotSystemManager.Tree;
            GameObject target = mGameObject;
            Transform container = mGameObject.transform.Find("ReddotContainer");
            if (container != null) target = container.gameObject;
            var parentType = BasePrimitivePanel.Instance.mPrimitiveRedDotSystemManager.NodeTypeDict[type];
            VNode vNode = tree.AddRedDot(target, (int)parentType, (int)ENodeType.PrimitiveItem, ERedDotPrefabType.Type4);
            return vNode;
        }
    }

    public Transform PriParent;
    public Action<int> OnSelect;
    private SpriteAtlas priAtlas;
    private GameObject priPrefab;
    private List<GameObject> allSelect = new List<GameObject>();
    private static int selectIndex = -1;
    private static List<int> resIDs = new List<int>();
    public RedDotSystem mRedDotSystem;
    public Dictionary<int,PrimitiveItem> mPrimitiveItems;
    public PrimitiveRedDotManager mPrimitiveRedDotSystemManager;
    public Button CharacterBtn, GeneralBtn, GamePlayBtn, SceneBtn;
    public GameObject typeBg;
    public Dictionary<PrimitiveItemType, Button> typeBtnDict = new Dictionary<PrimitiveItemType, Button>();
    public bool isGameEditScene = true;
    public static void SetResIDs(List<int> ids)
    {
        resIDs = ids;
    }

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        mPrimitiveItems = new Dictionary<int, PrimitiveItem>();
        typeBtnDict.Add(PrimitiveItemType.Character, CharacterBtn);
        typeBtnDict.Add(PrimitiveItemType.General, GeneralBtn);
        typeBtnDict.Add(PrimitiveItemType.GamePlay, GamePlayBtn);
        typeBtnDict.Add(PrimitiveItemType.Scene, SceneBtn);
        CharacterBtn.onClick.AddListener(() =>
        {
            ShowPrimitiveItems(PrimitiveItemType.Character);
        });

        GeneralBtn.onClick.AddListener(() =>
       {
           ShowPrimitiveItems(PrimitiveItemType.General);
       });

        GamePlayBtn.onClick.AddListener(() =>
        {
            ShowPrimitiveItems(PrimitiveItemType.GamePlay);
        });

        SceneBtn.onClick.AddListener(() =>
       {
           ShowPrimitiveItems(PrimitiveItemType.Scene);
       });
        priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        priPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "BasePrimitiveItem");

        //不属于白名单人员时，移除白名单专属的item
        if (GameManager.Inst.isInWhiteList == 0)
        {
            for (int i = 0; i < GameManager.Inst.priData.Count; i++)
            {
                var data = GameManager.Inst.priData[i];
                if (data.isWhiteListOnly == 1 && resIDs.Contains(data.id))
                {
                    resIDs.Remove(data.id);
                }
            }
        }

        for (int i = 0; i < resIDs.Count; i++)
        {
            int index = i;
            var itemGo = Instantiate(priPrefab, PriParent);
            itemGo.SetActive(true);
            var selectGo = itemGo.transform.GetChild(1).gameObject;
            selectGo.gameObject.SetActive(false);
            allSelect.Add(selectGo);
            var itemScript = itemGo.transform.GetChild(0).GetComponent<Image>();
            var itemBtn = itemGo.transform.GetChild(0).GetComponent<Button>();
            int itemId = resIDs[index];
            var itemData = GameManager.Inst.priConfigData[itemId];
            string iconName = itemData.iconName;
            itemScript.sprite = priAtlas.GetSprite(iconName);
            PrimitiveItem item = new PrimitiveItem(index, itemId, itemGo);
            if (itemData.propType != (int)PrimitiveItemType.NONE)
            {
                item.type = (PrimitiveItemType)itemData.propType;
            }
            item.Init();
            item.mOnClickFunc = CreatePrimitive;
            mPrimitiveItems.Add(itemId,item);
            //TODO:临时红点代码，红点系统完成，需要删掉
            //CreateRedDot(itemId,itemGo);
        }
        mPrimitiveRedDotSystemManager = new PrimitiveRedDotManager(this);
        mPrimitiveRedDotSystemManager.ConstructRedDotSystem();
        mPrimitiveRedDotSystemManager.RequestRedDot();
        mPrimitiveRedDotSystemManager.AddListener(RedDotInitedCallBack);
        UpdateUI();
    }

    public void UpdateUI()
    {
        var type = PrimitiveItemType.General;
        if (!isGameEditScene)
        {
            type = PrimitiveItemType.Scene;
        }
        ShowPrimitiveItems(type);
        typeBg.SetActive(isGameEditScene);
    }
    private void RedDotInitedCallBack(bool isInited)
    {
        if (isInited)
        {
            InternalAttachTypeRedDotNode();
            List<int> redDotId = mPrimitiveRedDotSystemManager.mPrimitiveIds;
            for (int i = 0; i < redDotId.Count; i++)
            {
                int id = redDotId[i];
                PrimitiveItem item;
                if (mPrimitiveItems.TryGetValue(id, out item))
                {
                    item.AttachRedDotNode();
                }
            }
        }
    }
    
    private void CreatePrimitive(int index)
    {
        HighLight(index);
        int itemId = resIDs[index];
        OnSelect?.Invoke(itemId);
    }

    public void OnIconSelect(int id)
    {
        if (id == (int)GameResType.PortalButton)
        {
            id = (int)GameResType.PortalPoint;
        }
        if (PGCPlantManager.Inst.IsPGCPlant(id))//PGC植物ID为11000——11999
        {
            id = (int)GameResType.PGCPlant;
        }
        if(VIPZoneManager.Inst.IsVIPZone(id)) // VIP 区域 ID为1070-1080
        {
            id = (int)GameResType.VIPZone;
        }
        if (PGCEffectManager.Inst.IsPGCEffect(id)) // PGC 特效 ID为16001-16100
        {
            id = (int)GameResType.PGCEffect;
        }
        DisHighlight();
        int index = resIDs.FindIndex(x=>x==id);
        if (index >= 0)
        {
            HighLight(index);
        }

        CheckRedDot(id);

    }


    private void HighLight(int index)
    {
        if (index != selectIndex)
        {
            DisHighlight();
            selectIndex = index;
        }
        allSelect[selectIndex].SetActive(true);

        int itemId = resIDs[index];
        PrimitiveItem item;
        if (mPrimitiveItems.TryGetValue(itemId, out item))
        {
            if (item.type != PrimitiveItemType.NONE)
            {
                ShowPrimitiveItems(item.type);
            }
        }
    }

    //根据选中的分类显示 Items
    private void ShowPrimitiveItems(PrimitiveItemType type)
    {
        foreach (var item in mPrimitiveItems.Values)
        {
            if (item.type != type)
            {
                item.mGameObject.SetActive(false);
            }
            else
            {
                item.mGameObject.SetActive(true);
            }
        }
        OnClickTyPe(type);
    }

    public void OnClickTyPe(PrimitiveItemType type)
    {
        var selectBtn = typeBtnDict[type];
        foreach (var btn in typeBtnDict.Values)
        {
            var select = btn.transform.Find("select");
            var selectBg = btn.transform.Find("selectBg");
            var normalBg = btn.transform.Find("normalBg");
            bool isSelected = selectBtn == btn;
            select.gameObject.SetActive(isSelected);
            selectBg.gameObject.SetActive(isSelected);
            normalBg.gameObject.SetActive(!isSelected);
        }
    }

    public void DisHighlight()
    {
        if (selectIndex >= 0 && selectIndex < allSelect.Count)
        {
            allSelect[selectIndex].SetActive(false);
        }
        selectIndex = -1;
    }

    public static void DisSelect()
    {
        if (Instance != null)
        {
            Instance.DisHighlight();
        }
    }

    private void InternalAttachTypeRedDotNode()
    {
        AttachPrimitiveRedDotNode(CharacterBtn.gameObject, PrimitiveItemType.Character);
        AttachPrimitiveRedDotNode(GeneralBtn.gameObject, PrimitiveItemType.General);
        AttachPrimitiveRedDotNode(GamePlayBtn.gameObject, PrimitiveItemType.GamePlay);
        AttachPrimitiveRedDotNode(SceneBtn.gameObject, PrimitiveItemType.Scene);
    }

    public VNode AttachPrimitiveRedDotNode(GameObject target, PrimitiveItemType type)
    {
        RedDotTree tree = BasePrimitivePanel.Instance.mPrimitiveRedDotSystemManager.Tree;
        Transform container = target.transform.Find("ReddotContainer");
        if (container != null) target = container.gameObject;
        var nodeType = mPrimitiveRedDotSystemManager.NodeTypeDict[type];
        VNode vNode = tree.AddRedDot(target, (int)ENodeType.Root, (int)nodeType, ERedDotPrefabType.Type4);
        return vNode;
    }

    private void CreateRedDot(int itemId,GameObject itemNode)
    {
        if (itemId == (int)GameResType.BornPoint)
        {
            RedDotManager.Inst.CreateRedItemNode(RedDotKey.SPAWN_UPGRADE,itemNode,new Vector2(40,40));
        }
        else if(itemId == (int)GameResType.TrapBox)
        {
            RedDotManager.Inst.CreateRedItemNode(RedDotKey.TRAP_BOX_UPGRADE,itemNode,new Vector2(40,40));
        }
        else if(itemId == (int)GameResType.IceCube)
        {
            RedDotManager.Inst.CreateRedItemNode(RedDotKey.ICE_CUBE_UPGRADE,itemNode,new Vector2(40,40));
        }
        else if (itemId == (int)GameResType.Firework)
        {
            RedDotManager.Inst.CreateRedItemNode(RedDotKey.FIREWORK_UPGRADE, itemNode, new Vector2(40, 40));
        }
    }


    private void CheckRedDot(int itemId)
    {
        if (itemId == (int)GameResType.BornPoint)
        {
            RedDotManager.Inst.SaveOnceRedDot(RedDotKey.SPAWN_UPGRADE);
        }
        else if(itemId == (int)GameResType.TrapBox)
        {
            RedDotManager.Inst.SaveOnceRedDot(RedDotKey.TRAP_BOX_UPGRADE);
        }
        else if(itemId == (int)GameResType.IceCube)
        {
            RedDotManager.Inst.SaveOnceRedDot(RedDotKey.ICE_CUBE_UPGRADE);
        }
        else if (itemId == (int)GameResType.Firework)
        {
            RedDotManager.Inst.SaveOnceRedDot(RedDotKey.FIREWORK_UPGRADE);
        }
    }
}
