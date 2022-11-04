using Newtonsoft.Json;
using SavingData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ParachutePanel : InfoPanel<ParachutePanel>, IUndoRecord
{
    public Button closebtn;
    public Button btnChoosePropInNoPanel;
    public UgcPropChooseItem ugcChooseParachuteItem;
    public UgcPropChooseItem UgcChoosebagItem;
    public GameObject goHasPanel;
    public GameObject goNoPanel;
    public Button closePanelBtn;
    public Image isSelectPara;
    public Button editParaBtn;
    public Button ParaPointBtn;
    public Text ParaPointText;

    public Image isSelectParaBag;
    public Button editParaBagBtn;
    public Button ParaBagPointBtn;
    public Text ParaBagPointText;

    private NodeBaseBehaviour parachuteBehav;
    private NodeBaseBehaviour bagBehav;
    private SceneEntity parachuteEntity;
    private SceneEntity bagEntity;
    private int oldBagId;
    private int oldParaId;

    private const string parachuteTitel = "The parachute will be attached to the player at the hang point you set";
    private const string parachuteNodeName = "ParachuteAnchos";
    private const string parachuteBagTitel = "The parachute bag will be attached to the player at the hang point you set";
    private const string paraBagchuteNodeName = "ParachuteBagAnchos";

    public override void OnInitByCreate()
    {
        BtnInit();
        ParachuteInit();
        BagInit();
    }

    public void BtnInit()
    {
        btnChoosePropInNoPanel.onClick.AddListener(ugcChooseParachuteItem.OpenUgcBagpackAndChoose);
        ParaPointBtn.onClick.AddListener(OnParaPointBtnClick);
        editParaBtn.onClick.AddListener(()=> {
            OnEditClick(parachuteTitel, parachuteNodeName, parachuteBehav, OnSureClick, OnCancelClick);
        });
        ParaBagPointBtn.onClick.AddListener(OnParaBagPointBtnClick);
        editParaBagBtn.onClick.AddListener(()=> {
            OnEditClick(parachuteBagTitel, paraBagchuteNodeName, bagBehav, OnSureClick, OnCancelClick);
        });
        closebtn.onClick.AddListener(OnCloseBtnClick);

        closePanelBtn.onClick.AddListener(OnCloseBtnClick);
    }


    #region 降落伞逻辑
    public void ParachuteInit()
    {
        ugcChooseParachuteItem.OnInitByCreate();
        ugcChooseParachuteItem.AfterUgcCreateFinishCallback = (behav, id) =>
        {
            RefreshUI();
            var paraEntity = parachuteBehav.entity;
            int bagid = oldBagId;
            ParachuteManager.Inst.AddParachuteComponent(behav, id, bagid);
            ugcChooseParachuteItem.allUsedUgcs = ParachuteManager.Inst.GetAllUgcRidList(ParaUgcType.Parachute);
            bagBehav = ParachuteManager.Inst.ByIdFindBehav(bagid,ParachuteManager.Inst.allBagUgcDict);
            if (bagBehav != null)
            {
                bagBehav.entity.Get<ParachuteBagComponent>().parachuteUid = behav.entity.Get<GameObjectComponent>().uid;
            }
            ParachuteCreater.AddConstrainer(behav);
            ParachuteManager.Inst.AddPickableComponent(behav,ParaUgcType.Parachute);

            // EditModeController.SetSelect?.Invoke(behav.entity);
        };
    }
    public void SetParachuteEntity(SceneEntity entity)
    {
        BindPointInit();
        ugcChooseParachuteItem.allUsedUgcs = ParachuteManager.Inst.GetAllUgcRidList(ParaUgcType.Parachute);
        parachuteEntity = entity;
        if (parachuteEntity.HasComponent<ParachuteComponent>())
        {
            oldBagId = parachuteEntity.Get<ParachuteComponent>().parachuteBagUid;
        }
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        parachuteBehav = entityGo.GetComponent<NodeBaseBehaviour>();
        if (parachuteBehav is ParachuteBehaviour) //默认道具模型
        {
            ugcChooseParachuteItem.ChooseItem(string.Empty);
        }
        else if (parachuteBehav is UGCCombBehaviour && parachuteEntity.HasComponent<ParachuteComponent>()) //UGC道具
        {
            string rId = parachuteEntity.Get<ParachuteComponent>().rid;
            ugcChooseParachuteItem.ChooseItem(rId);
            var isSelect = parachuteEntity.Get<ParachuteComponent>().isCustomPoint == (int)CustomPointState.Off ? false : true;
            SetPointState(isSelect, isSelectPara, editParaBtn);
            SetPointColor(parachuteEntity.Get<GameObjectComponent>().resId, ParaPointText, ParaPointBtn);
        }
        ugcChooseParachuteItem.SetEntity(parachuteBehav);
        RefreshUI();
        //选中降落伞后，根据降落伞的条件来设置背包
        OnChooseParaSetBag();
        ParachuteManager.Inst.HideAllBagActive();
        ParachuteManager.Inst.ShowBag(parachuteEntity);
    }
    #endregion


    #region 背包逻辑
    public void BagInit()
    {
        UgcChoosebagItem.OnInitByCreate();
        UgcChoosebagItem.AfterUgcCreateFinishCallback = (behav, id) =>
        {
            var oldEntity = bagBehav.entity;
            int paraId = oldParaId;
            ParachuteManager.Inst.AddParachuteBagComponent(behav, id, paraId);
            UgcChoosebagItem.allUsedUgcs = ParachuteManager.Inst.GetAllUgcRidList(ParaUgcType.Bag);
            var paraBehav = ParachuteManager.Inst.ByIdFindBehav(behav.entity.Get<ParachuteBagComponent>().parachuteUid,ParachuteManager.Inst.allParaUgcDict);
            if(paraBehav != null){
                paraBehav.entity.Get<ParachuteComponent>().parachuteBagUid = behav.entity.Get<GameObjectComponent>().uid;
                UgcChoosebagItem.curBehav = behav;
            }
            bagBehav = behav;
            bagEntity = behav.entity;
            ParachuteBagCreater.AddConstrainer(behav);
            ParachuteManager.Inst.AddPickableComponent(behav, ParaUgcType.Bag);

            // EditModeController.SetSelect?.Invoke(behav.entity);
        };
    }
    public void SetBagEntity(SceneEntity entity)
    {
        bagEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        bagBehav = entityGo.GetComponent<NodeBaseBehaviour>();
        if (bagEntity.HasComponent<ParachuteBagComponent>())
        {
            oldParaId = bagEntity.Get<ParachuteBagComponent>().parachuteUid;
        }
        if (bagBehav is ParachuteBagBehaviour)
        {
            UgcChoosebagItem.ChooseItem(string.Empty);
        }
        else if (bagBehav is UGCCombBehaviour && entity.HasComponent<ParachuteBagComponent>())
        {
            string rId = entity.Get<GameObjectComponent>().resId;
            UgcChoosebagItem.ChooseItem(rId);
        }
        UgcChoosebagItem.SetEntity(bagBehav);
    }

    private void OnChooseParaSetBag()
    {
        if (parachuteEntity.HasComponent<ParachuteComponent>() && parachuteEntity.Get<ParachuteComponent>().rid != ParachuteManager.DEFAULT_MODEL)
        {
            var bagId = parachuteEntity.Get<ParachuteComponent>().parachuteBagUid;
            bagBehav = ParachuteManager.Inst.ByIdFindBehav(bagId, ParachuteManager.Inst.allBagUgcDict);
            if (bagBehav == null)
            {
                bagBehav = CreateBag(parachuteEntity);
                bagEntity = bagBehav.entity;
                bagBehav.gameObject.SetActive(true);
            }
            else
            {
                bagEntity = bagBehav.entity;
                var paraUid = bagEntity.Get<ParachuteBagComponent>().parachuteUid;
                if (parachuteEntity.Get<GameObjectComponent>().uid != paraUid)
                {
                    bagBehav = CreateBag(parachuteEntity);
                    bagEntity = bagBehav.entity;
                }
            }
            var isSelect = bagEntity.Get<ParachuteBagComponent>().isCustomPoint == (int)CustomPointState.Off ? false : true;
            SetPointState(isSelect, isSelectParaBag, editParaBagBtn);
            SetPointColor(bagEntity.Get<GameObjectComponent>().resId, ParaBagPointText, ParaBagPointBtn);
            UgcChoosebagItem.allUsedUgcs = ParachuteManager.Inst.GetAllUgcRidList(ParaUgcType.Bag);
            SetBagEntity(bagBehav.entity);
        }
    }

    ///生成新的背包，重新对全局behav和entity赋值，互相绑定uid 
    public NodeBaseBehaviour CreateBag(SceneEntity paraEntity)
    {
        var pos = parachuteBehav.transform.position + ParachuteManager.Inst.bagOffset;
        bagBehav = ParachuteManager.Inst.CreateParachuteBagBeavInEdit(pos);
        bagEntity = bagBehav.entity;
        var comp = bagEntity.Get<ParachuteBagComponent>();
        comp.parachuteUid = parachuteEntity.Get<GameObjectComponent>().uid;
        paraEntity.Get<ParachuteComponent>().parachuteBagUid = bagEntity.Get<GameObjectComponent>().uid;
        return bagBehav;
    }
    #endregion


    #region ParaPoint
    private void OnParaPointBtnClick()
    {
        if (!parachuteEntity.HasComponent<ParachuteComponent>())
        {
            return;
        }
        var comp = parachuteEntity.Get<ParachuteComponent>();
        if (comp.rid == ParachuteManager.DEFAULT_MODEL)
        {
            TipPanel.ShowToast("Please set the object first");
            return;
        }
        if (comp.isCustomPoint == (int)CustomPointState.Off)
        {
            OnEditClick(parachuteTitel, parachuteNodeName, parachuteBehav, OnSureClick, OnCancelClick);
        }
        else
        {
            SetPointState(false, isSelectPara, editParaBtn);
            comp.isCustomPoint = (int)CustomPointState.Off;
            ParachuteManager.Inst.SetAnchors(parachuteEntity, Vector3.zero);
        }
    }

    private void OnParaBagPointBtnClick()
    {
        if (bagEntity == null || !bagEntity.HasComponent<ParachuteBagComponent>())
        {
            return;
        }
        var comp = bagEntity.Get<ParachuteBagComponent>();
        if (comp.rid == ParachuteManager.DEFAULT_MODEL)
        {
            TipPanel.ShowToast("Please set the object first");
            return;
        }
        if (comp.isCustomPoint == (int)CustomPointState.Off)
        {
            OnEditClick(parachuteBagTitel, paraBagchuteNodeName, bagBehav, OnSureClick, OnCancelClick);
        }
        else
        {
            SetPointState(false, isSelectParaBag, editParaBagBtn);
            comp.isCustomPoint = (int)CustomPointState.Off;
            ParachuteManager.Inst.SetAnchors(bagEntity, Vector3.zero);
        }
    }

    private void OnEditClick(string title, string nodeName, NodeBaseBehaviour nBehav, Action<Vector3, SceneEntity> SureBtnClickAct, Action<Vector3, SceneEntity> CancelBtnClickAct)
    {
        if (nBehav == null)
        {
            return;
        }
        SceneEntity entity = nBehav.entity;
        CommonEditAnchorsPanel.Show();
        CommonEditAnchorsPanel.Instance.SetTitle(title);
        var pos = ParachuteManager.Inst.GetAnchors(entity);
        CommonEditAnchorsPanel.Instance.Init(entity, pos);
        CommonEditAnchorsPanel.Instance.SetNodeName(nodeName);
        CommonEditAnchorsPanel.Instance.SureBtnClickAct = (pointPos) => { SureBtnClickAct(pointPos, entity); };
        CommonEditAnchorsPanel.Instance.CancelBtnClickAct = (pointPos) => { CancelBtnClickAct(pointPos, entity); };
        nBehav.gameObject.SetActive(true);
    }

    public void OnSureClick(Vector3 pos, SceneEntity entity)
    {
        if (entity.HasComponent<ParachuteComponent>())
        {
            SetPointState(true, isSelectPara, editParaBtn);
            entity.Get<ParachuteComponent>().isCustomPoint = (int)CustomPointState.On;
        }
        else if (entity.HasComponent<ParachuteBagComponent>())
        {
            SetPointState(true, isSelectParaBag, editParaBagBtn);
            entity.Get<ParachuteBagComponent>().isCustomPoint = (int)CustomPointState.On;
        }
        ParachuteManager.Inst.SetAnchors(entity, pos);
        SceneEditModeController.SetSelect?.Invoke(entity);
    }
    public void OnCancelClick(Vector3 pos, SceneEntity entity)
    {
        pos = ParachuteManager.Inst.GetAnchors(entity);
        SceneEditModeController.SetSelect?.Invoke(entity);
    }

    private void SetPointColor(string rid,Text pointText,Button pointBtn)
    {
        float value = (rid == ParachuteManager.DEFAULT_MODEL) || (string.IsNullOrEmpty(rid)) ? 0.6f : 1f;
        var tempColor = pointText.color;
        tempColor.a = value;
        pointText.color = tempColor;
        pointBtn.image.color = tempColor;
    }

    private void SetPointState(bool isSelect,Image selectPara,Button editBtn)
    {
        selectPara.gameObject.SetActive(isSelect);
        editBtn.gameObject.SetActive(isSelect);
    }
    #endregion

    //刷新UI
    private void RefreshUI()
    {
        var isActive = parachuteBehav is ParachuteBehaviour ? false : true;
        goNoPanel.SetActive(!isActive);
        goHasPanel.SetActive(isActive);
    }

    public void OnCloseBtnClick()
    {
        var opanel = UIManager.Inst.uiCanvas.GetComponentsInChildren<IPanelOpposable>(true);
        for (int i = 0; i < opanel.Length; i++)
        {
            opanel[i].SetGlobalHide(true);
        }
        this.gameObject.SetActive(false);
    }

    private void BindPointInit()
    {
        SetPointState(false, isSelectParaBag, editParaBagBtn);
        SetPointColor(ParachuteManager.DEFAULT_MODEL, ParaBagPointText, ParaBagPointBtn);
        SetPointState(false, isSelectPara, editParaBtn);
        SetPointColor(ParachuteManager.DEFAULT_MODEL, ParaPointText, ParaPointBtn);
    }

    public void AddRecord(UndoRecord record)
    {
    }
}
