using System.Collections.Generic;
using Amazon.Util.Storage;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class FreezePropsPanel : UgcChoosePanel<FreezePropsPanel>
{
    private SceneEntity mCurEntity;
    public Text mFreezeTimeText;
    public Button mInputBtn;
    public Button btnPlus;
    public Button btnSub;

    private int mCurFreezeTime = 0;
    private FreezePropsManager mManager;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        mManager = FreezePropsManager.Inst;
        mInputBtn.onClick.AddListener(OnShowKeyBoard);
        btnPlus.onClick.AddListener(OnBtnPlusClick);
        btnSub.onClick.AddListener(OnBtnSubClick);
        mCurFreezeTime = FreezePropsManager.mNormalFreezeTime;
        mFreezeTimeText.text = FreezePropsManager.mNormalFreezeTime.ToString();

    }
    private void OnBtnPlusClick()
    {
        if (mCurFreezeTime >= FreezePropsManager.mFreezeTimeMax)
        {
            TipPanel.ShowToast("Up to {0}", FreezePropsManager.mFreezeTimeMax);
            return;
        }
        mCurFreezeTime += 1;
        SetEntityAtt();
    }

    private void OnBtnSubClick()
    {
        if (mCurFreezeTime <= FreezePropsManager.mFreezeTimeMin)
        {
            TipPanel.ShowToast("At least {0}", FreezePropsManager.mFreezeTimeMin);
            return;
        }
        mCurFreezeTime -= 1;
        SetEntityAtt();
    }
    private void OnShowKeyBoard()
    {
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = mCurFreezeTime.ToString(),
            inputMode = 1,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = "Oops! Exceed limit:(",
            defaultText = "",
            returnKeyType = (int)ReturnType.Return
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, ShowKeyBoard);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }
    public void ShowKeyBoard(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        int freezeTime;
        if (!int.TryParse(str, out freezeTime))
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        if (freezeTime > 30 || freezeTime < 2)
        {
            TipPanel.ShowToast("Please enter the correct value");
            return;
        }
        mCurFreezeTime = freezeTime;
        mCurEntity.Get<FreezePropsComponent>().mFreezeTime = mCurFreezeTime;
        mFreezeTimeText.text = mCurFreezeTime.ToString();
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }
    private void SetEntityAtt()
    {
        mCurEntity.Get<FreezePropsComponent>().mFreezeTime = mCurFreezeTime;
        mFreezeTimeText.text = mCurFreezeTime.ToString();
    }
    public override void SetEntity(SceneEntity entity)
    {
        mCurEntity = entity;
        RefreshUI();
        curBehav = entity.Get<GameObjectComponent>().bindGo.GetComponent<NodeBaseBehaviour>();
        if (curBehav is FreezePropsBehaviour)
        {
            //Ĭ��ռλ��Ѫ����
            ChooseItem(string.Empty);
        }
        else if (curBehav is UGCCombBehaviour && entity.HasComponent<FreezePropsComponent>())
        {
            //UGC��Ѫ����
            string rId = entity.Get<FreezePropsComponent>().rId;
            ChooseItem(rId);
        }
        mCurFreezeTime = (int)entity.Get<FreezePropsComponent>().mFreezeTime;
        mFreezeTimeText.text = mCurFreezeTime.ToString();
    }
    protected override void RefreshUI()
    {
        base.RefreshUI();
        var allUsedUgcs = GetAllUgcRidList();
        if (allUsedUgcs != null)
        {
            goNoPanel.SetActive(allUsedUgcs.Count > 0 == false);
            goHasPanel.SetActive(allUsedUgcs.Count > 0);
        }
    }
    protected override List<string> GetAllUgcRidList()
    {
        return FreezePropsManager.Inst.GetAllUgcRidList();
    }
    public override void SetLastChooseUgcItem(UgcChooseItem Item)
    {
        mManager.LastSelect = Item;
    }
    protected override void ChooseUgcCallback(MapInfo mapInfo)
    {
        base.ChooseUgcCallback(mapInfo);

    }
    protected override void AfterUgcCreateFinish(NodeBaseBehaviour nBehav, string rId)
    {
        if (nBehav == null) return;
        nBehav.transform.parent = SceneBuilder.Inst.StageParent;
        FreezePropsManager.Inst.AddConstrainer(nBehav);
        var gameComponent = nBehav.entity.Get<GameObjectComponent>();
        gameComponent.handleType = NodeHandleType.FreezeProps;
        gameComponent.modelType = NodeModelType.FreezeProps;

        FreezePropsNodeAuxiliary auxiliary = new FreezePropsNodeAuxiliary(nBehav, FreezePropsManager.Inst);
        FreezePropsManager.Inst.AddNodeAuxiliary(nBehav, auxiliary);
        auxiliary.UpdatePropBehaviour(nBehav, false);

        FreezePropsManager.Inst.AddAndSetComponent(nBehav, rId);
        FreezePropsManager.Inst.AddUgcItem(rId, nBehav);
    }


    public override void AddTabListener()
    {

    }
}