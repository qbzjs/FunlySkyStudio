using System.Collections.Generic;
using Newtonsoft.Json;
using SavingData;
using UnityEngine;

public class PlayerPromoteController : MonoBehaviour
{
    public bool InSelect { get { return _status == PromoteStatus.Select; } }
    public bool InPromote { get { return _status != PromoteStatus.None && _status != PromoteStatus.Select && _status != PromoteStatus.End; } }
    public PromoteStatus Status  { get{ return _status; } }

    private string _playerId;
    private PromoteStatus _status = PromoteStatus.None;

    private Transform _promotePos;
    private Transform _itemParent;

    private GameObject _notebookGo;
    private GameObject _carpetGo;
    private GameObject _trumpetGo;
    private GameObject _smokeGo;
    private GameObject _carpetEffectGo;

    private Animator _carpetAnimator;
    private Animator _notebookAnimator;
    private Animator _trumpetAnimator;

    private BudTimer _animationTimer;
    private BudTimer _buildItemTimer;
    private AnimationController _animtionCtrl;
    private List<PromoteItemInfo> _itemInfos;

    private List<Vector3[]> _posLst = new List<Vector3[]>
    {
        new Vector3[] { new Vector3(0f, 0.25f, 0.7f) },
        new Vector3[] { new Vector3(-0.3f, 0.25f, 0.7f), new Vector3(0.3f, 0.25f, 0.7f) },
        new Vector3[] { new Vector3(-0.75f, 0.25f, 0.7f), new Vector3(0f, 0.25f, 0.7f), new Vector3(0.75f, 0.25f, 0.7f) },
        new Vector3[] { new Vector3(-0.75f, 0.25f, 0.7f), new Vector3(0f, 0.25f, 0.7f), new Vector3(0.75f, 0.25f, 0.7f), new Vector3(0f, 0.25f, 1.2f) },
        new Vector3[] { new Vector3(-0.75f, 0.25f, 0.7f), new Vector3(0f, 0.25f, 0.7f), new Vector3(0.75f, 0.25f, 0.7f), new Vector3(-0.3f, 0.25f, 1.2f), new Vector3(0.3f, 0.25f, 1.2f) },
        new Vector3[] { new Vector3(-0.75f, 0.25f, 0.7f), new Vector3(0f, 0.25f, 0.7f), new Vector3(0.75f, 0.25f, 0.7f), new Vector3(-0.75f, 0.25f, 1.2f), new Vector3(0f, 0.25f, 1.2f), new Vector3(0.75f, 0.25f, 1.2f) },
     };

    private Vector3 _targetSize = new Vector3(0.75f, 0.75f, 0.75f);

    private bool IsMyself { get { return _playerId == GameManager.Inst.ugcUserInfo.uid; } }

    private void Awake()
    {
        _animtionCtrl = GetComponent<AnimationController>();

        var playerModel = _animtionCtrl.playerModle;
        _promotePos = playerModel.transform.Find("PromotePos");
        _itemParent = _promotePos.Find("ItemParent");

        var notebookPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Promote/Notebook");
        var carpetPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Promote/Carpet");
        var trumpetPrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Promote/Trumpet");
        var smokePrefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Promote/Smoke");

        _notebookGo = GameObject.Instantiate(notebookPrefab, _promotePos);
        _carpetGo = GameObject.Instantiate(carpetPrefab, _promotePos);
        _trumpetGo = GameObject.Instantiate(trumpetPrefab, _promotePos);
        _smokeGo = GameObject.Instantiate(smokePrefab, _promotePos);
        _carpetEffectGo = _carpetGo.transform.Find("stall_standby").gameObject;

        _notebookAnimator = _notebookGo.GetComponent<Animator>();
        _carpetAnimator = _carpetGo.GetComponent<Animator>();
        _trumpetAnimator = _trumpetGo.GetComponent<Animator>();

        _notebookGo.SetActive(false);
        _carpetGo.SetActive(false);
        _trumpetGo.SetActive(false);
        _smokeGo.SetActive(false);
        _carpetEffectGo.SetActive(false);
    }

    private void OnDestroy()
    {
        StopAnimationTimer();
        StopBuildItemTimer();
    }

    // 初始化
    public void Init(string playerId)
    {
        _playerId = playerId;
    }

    public void Select()
    {
        // 先重置，确保状态干净
        ResetToDefault();

        _status = PromoteStatus.Select;

        // 播放开始选货动画
        PlayPlayerAnimation("PromoteSelectStart");

        // 播放选货音效
        PlayPromoteAudio("Think");

        // 播放笔记本出现动画
        _notebookGo.SetActive(true);
        _notebookAnimator.Play("Begin");
    }

    public void ExitSelect()
    {
        _status = PromoteStatus.None;

        // 播放选货结束动画
        PlayPlayerAnimation("PromoteSelectEnd");

        // 播放笔记本结束动画
        _notebookAnimator.Play("End");

        // 获取笔记本收起动画的时长
        var notebookEndtime = GetAnimationLength(_notebookAnimator, "consider_end");

        StopAnimationTimer();
        _animationTimer = TimerManager.Inst.RunOnce("PromoteTimer", notebookEndtime, () => {
            // 隐藏笔记本
            _notebookGo.SetActive(false);

            // 回到常规Idle动画
            PlayPlayerAnimation("idle");
        });
    }

    public void BeginPromote(List<PromoteItemInfo> itemInfos)
    {
        _status = PromoteStatus.Begin;

        _itemInfos = itemInfos;

        // 如果是我自己，显示带货界面
        if (IsMyself)
            ShowPromotePanel();

        // 播放选货结束动画
        PlayPlayerAnimation("PromoteSelectEnd");

        // 播放笔记本结束动画
        _notebookAnimator.Play("End");

        // 获取笔记本收起动画的时长
        var notebookEndTime = GetAnimationLength(_notebookAnimator, "consider_end");

        // 等待选货结束动画播放完成后
        StopAnimationTimer();
        _animationTimer = TimerManager.Inst.RunOnce("PromoteTimer", notebookEndTime, () => {
            // 隐藏笔记本
            _notebookGo.SetActive(false);

            // 播放摆摊动画
            PlayPlayerAnimation("PromoteBegin");

            // 播放摆摊音效
            PlayPromoteAudio("Set");

            // 播放地毯出现动画
            _carpetGo.SetActive(true);
            _carpetEffectGo.SetActive(true);
            _carpetAnimator.Play("Begin");

            // 获取地毯展开动画的时长
            var carpetOpenTime = GetAnimationLength(_carpetAnimator, "stall_tanzi@start");
            BuildItems(carpetOpenTime);
        });
    }

    // 进房时直接刷数据用的接口
    public void BeginPromoteImmediate(List<PromoteItemInfo> itemInfos)
    {
        _status = PromoteStatus.Begin;

        _itemInfos = itemInfos;

        // 如果是我自己，显示带货界面
        if (IsMyself)
            ShowPromotePanel();

        StopAnimationTimer();

        // 显示地毯并播放地毯Idle动画
        _carpetGo.SetActive(true);
        _carpetAnimator.Play("Idle");

        BuildItems(0);
    }

    public void ExitPromote()
    {
        _status = PromoteStatus.End;

        // 销毁所有商品
        ClearItems();

        // 播放收摊动画
        PlayPlayerAnimation("PromoteEnd");

        // 播放收摊音效
        PlayPromoteAudio("Pack");

        // 隐藏地毯粒子特效
        _carpetEffectGo.SetActive(false);

        // 隐藏喇叭
        _trumpetGo.SetActive(false);

        // 显示收摊烟雾特效
        _smokeGo.SetActive(true);

        // 播放地毯结束动画
        _carpetAnimator.Play("End");

        // 获取地毯收摊动画的时长
        var carpetEndTime = GetAnimationLength(_carpetAnimator, "stall_tanzi@end");

        // 等待收摊动画播放完成后
        StopAnimationTimer();
        _animationTimer = TimerManager.Inst.RunOnce("PromoteTimer", carpetEndTime, () => {
            ResetToDefault();
        });
    }

    public void Introduce()
    {
        _status = PromoteStatus.Introduce;

        // 播放介绍动画
        PlayPlayerAnimation("PromoteIntroduce");

        // 播放介绍音效
        PlayPromoteAudio("Explain");
    }

    public void Peddle()
    {
        _status = PromoteStatus.Peddle;

        // 播放吆喝动画
        PlayPlayerAnimation("PromotePeddle");

        // 播放吆喝音效
        PlayPromoteAudio("To_Sell");

        // 播放喇叭出现动画
        _trumpetGo.SetActive(true);
        _trumpetAnimator.Play("Peddle");

        // 获取喇叭吆喝动画的时长
        var trumpetPeddleTime = GetAnimationLength(_trumpetAnimator, "peddle");

        // 等待喇叭吆喝动画播放完成后
        StopAnimationTimer();
        _animationTimer = TimerManager.Inst.RunOnce("PromoteTimer", trumpetPeddleTime, () => {
            // 隐藏喇叭
            _trumpetGo.SetActive(false);
        });
    }

    public void ResetToDefault()
    {
        _status = PromoteStatus.None;

        // 停止计时器
        StopAnimationTimer();
        StopBuildItemTimer();

        // 确保销毁所有商品
        ClearItems();

        // 隐藏所有物件
        _notebookGo.SetActive(false);
        _carpetGo.SetActive(false);
        _trumpetGo.SetActive(false);
        _smokeGo.SetActive(false);

        // 回到常规Idle动画
        PlayPlayerAnimation("idle");

        // 如果是我自己，隐藏带货界面
        if (IsMyself)
            HidePromotePanel();
    }

    public bool CheckCustomer()
    {
        _status = PromoteStatus.CheckCustomer;

        float radius = 5;
        Collider[] curHits = Physics.OverlapSphere(this.transform.position, radius);
        foreach (var hit in curHits)
        {
            if (hit.gameObject.GetComponent<OtherPlayerCtr>())
                return true;
        }

        return false;
    }

    private void BuildItems(float delayTime)
    {
        ClearItems();

        StopBuildItemTimer();
        _buildItemTimer = TimerManager.Inst.RunOnce("PromoteTimer", delayTime, () => {
            if(_itemInfos.Count > _posLst.Count)
            {
                return;
            }
            var posLst = _posLst[_itemInfos.Count - 1];
            for (int i = 0; i < _itemInfos.Count; i++)
            {
                var itemInfo = _itemInfos[i];
                var pos = posLst[i];

                if (itemInfo.dataType == (int)DataTypeEnum.Resource)
                {
                    if (itemInfo.IsScenePgc())
                    {
                        var item = BuildPgcProp(itemInfo, pos);
                        PromoteManager.Inst.AddPromoteProp(item);
                        SetItemData(item, _itemParent, pos, itemInfo);
                    }
                    else
                    {
                        TipsGameObjCreater tipsGo = new TipsGameObjCreater();
                        GameObject tmpGo = tipsGo.CreateTipsGameObj(pos,_itemParent);
                         // 下载商品数据
                        MapLoadManager.Inst.LoadMapJson(itemInfo.propsJson, (content) => {
                        // 下载完后，只有商品还在摆摊中的话才会创建
                        if (PromoteManager.Inst.HasItemInfo(_playerId, itemInfo))
                        {
                            if(itemInfo.renderList != null)
                            {
                                // 创建商品
                                CheckRenderUrl(itemInfo.renderList);
                                UGCBehaviorManager.Inst.AddOfflineRenderData(itemInfo.renderList);
                                LoggerUtils.Log($"DelayBuildItems  :  rid:{itemInfo.mapId}, mapJsonContent:{content}");
                            }
                            var item = SceneBuilder.Inst.ParsePropAndBuild(content, transform.localPosition, itemInfo.mapId);
                            PromoteManager.Inst.AddPromoteProp(item);

                            if (itemInfo.isDC == 1)
                            {
                                var entity = item.entity;
                                var dcComp = entity.Get<DcComponent>();
                                var dcInfo = itemInfo.dcInfo;
                                if(dcInfo != null)
                                {
                                    dcComp.dcId = dcInfo.itemId;
                                    dcComp.isDc = itemInfo.isDC;
                                    dcComp.address = dcInfo.walletAddress;
                                    dcComp.budActId = dcInfo.budActId;
                                }
                            }
                            SetItemData(item, _itemParent, pos, itemInfo);
                            tipsGo.SetTipsParent(item as UGCCombBehaviour,tmpGo);
                        }
                        }, (error) =>
                        {
                            tipsGo.DestroyTipsGameObj();
                            LoggerUtils.LogError(string.Format("Load promote's item json failed. Error = {0}", error));
                        });
                    }
                   
                }
                else if (itemInfo.dataType == (int)DataTypeEnum.Clothing)
                {
                    // 创建商品
                    var item = SceneBuilder.Inst.ParseClothAndBuild(JsonConvert.SerializeObject(itemInfo));
                    var entity = item.entity;
                    var clothComp = entity.Get<UGCClothItemComponent>();
                    var dcInfo = itemInfo.dcInfo;
                    if (dcInfo != null)
                    {
                        clothComp.isDc = itemInfo.isDC;
                        clothComp.dcId = dcInfo.itemId;
                        clothComp.walletAddress = dcInfo.walletAddress;
                        clothComp.budActId = dcInfo.budActId;
                    }
                    SetItemData(item, _itemParent, pos, itemInfo);
                }
            }
        });
    }

    private void BroadGetSuccess(string owner, string goods, string url)
    {
        PurchasedTextData purchasedTextData = new PurchasedTextData()
        {
            goods = goods,
            source = owner,
            imgUrl = url
        };

        var purDataString = JsonConvert.SerializeObject(purchasedTextData);
        var textChatBev = PlayerEmojiControl.Inst.textCharBev;

        if(textChatBev != null)
        {
            textChatBev.SetPurchaseText(purDataString);
            PromoteManager.Inst.SendRequest(PromoteStatus.Purchased, purDataString);
        }
    }
    
    private NodeBaseBehaviour BuildPgcProp(PromoteItemInfo promoteItemInfo, Vector3 pos)
    {
        var dcInfo = promoteItemInfo.dcInfo;
        var nodeData = SceneBuilder.Inst.CrateScenePgcNodeData(
            new DCInfo(){itemId = dcInfo.itemId,
            walletAddress = dcInfo.walletAddress, budActId = dcInfo.budActId},
            promoteItemInfo.dcPgcInfo,
            promoteItemInfo.isDC, pos);
        return SceneBuilder.Inst.ParsePropAndBuild(JsonConvert.SerializeObject(nodeData), pos);
    }

    private void SetItemData(NodeBaseBehaviour baseBev, Transform parent, Vector3 pos, PromoteItemInfo itemInfo)
    {
        baseBev.transform.SetParent(parent);
        baseBev.transform.localRotation = Quaternion.identity;
        baseBev.transform.localPosition = pos;
        baseBev.gameObject.AddComponent<FloatCtrl>();
        baseBev.transform.eulerAngles += new Vector3(0, 180, 0);
        
        if (baseBev is PGCBehaviour pgc)
        {
            pgc.AddOnSetAssetAction(SetBevSize);
        }
        else if (baseBev is UgcClothItemBehaviour uci && itemInfo.IsClothPgc())
        {
            uci.AddOnSetAssetAction(SetBevSize);
        }
        else
        {
            PropSizeUtill.SetPropSize(_targetSize, baseBev);
            if (!IsMyself)
            {
                var entity = baseBev.entity;
                string mapName = itemInfo.mapName;
                string mapCover = itemInfo.mapCover;
                entity.Get<UGCPropComponent>().isTradable = 1;
                itemInfo.onGet = (sceneEntity)=>
                {
                    if(sceneEntity == entity) BroadGetSuccess(GameManager.Inst.ugcUserInfo.userName, mapName, mapCover);
                };
                StorePanel.onGet += itemInfo.onGet;
            }
            SetCollidersEnable(baseBev);
        }
    }

    private void SetBevSize(NodeBaseBehaviour bev)
    {
        PropSizeUtill.SetPropSize(_targetSize, bev);
        var entity = bev.entity;
        if (!IsMyself)
        {
            entity.Get<UGCPropComponent>().isTradable = 1;
        }
        else
        {
            entity.Get<UGCPropComponent>().isTradable = 0;
        }

        SetCollidersEnable(bev);
    }
    
    private void SetCollidersEnable(NodeBaseBehaviour baseBev)
    {
        var colliders = baseBev.transform.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
        // 设置层级为Touch层，进行射线检测
        var boxCollider = baseBev.transform.GetComponentInChildren<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.gameObject.layer = LayerMask.NameToLayer("Touch");
            boxCollider.enabled = true;
        }
    }

    private void ShowPromotePanel()
    {
        PromoteCtrPanel.Show();
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.OnPromoteModeChange(true);
        }
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(false);
        }
    }

    private void HidePromotePanel()
    {
        PromoteCtrPanel.Hide();
        if (PlayModePanel.Instance)
        {
            PlayModePanel.Instance.OnPromoteModeChange(false);
        }
        if (PortalPlayPanel.Instance)
        {
            PortalPlayPanel.Instance.SetPlayBtnVisible(true);
        }
    }

    // 销毁所有商品
    private void ClearItems()
    {
        for (int i = _itemParent.childCount - 1; i >= 0; i--)
        {
            
            var bindGo = _itemParent.GetChild(i).gameObject;
            var baseBev = bindGo.GetComponent<NodeBaseBehaviour>();
           
            if (baseBev != null)
            {
                PromoteManager.Inst.RemovePromoteProp(baseBev);
                var entity = baseBev.entity;
                if (entity.HasComponent<UGCClothItemComponent>())
                {
                    var clothBev = bindGo.GetComponent<UgcClothItemBehaviour>();
                    UgcClothItemManager.Inst.RemoveUgcClothItem(clothBev);
                }
                baseBev.OnReset();
            }
            SceneBuilder.Inst.DestroyEntity(bindGo);
        }
        
    }

    private void PlayPlayerAnimation(string name)
    {
        if (_animtionCtrl == null)
            return;

        _animtionCtrl.RleasePrefab();
        _animtionCtrl.CancelLastEmo();
        _animtionCtrl.PlayAnim(null, name);

        if (IsMyself)
        {
            PlayerBaseControl.Inst.SetPlayerRoleActive();
        }
    }

    private float GetAnimationLength(Animator animator, string name)
    {
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == name)
                return clip.length;
        }

        return 0;
    }

    private void PlayPromoteAudio(string audioName)
    {
        AKSoundManager.Inst.PlayAttackSound(audioName, "Play_Booth", "Booth", gameObject);
    }

    private void StopAnimationTimer()
    {
        if (_animationTimer != null)
        {
            TimerManager.Inst.Stop(_animationTimer);
            _animationTimer = null;
        }
    }

    private void StopBuildItemTimer()
    {
        if (_buildItemTimer != null)
        {
            TimerManager.Inst.Stop(_buildItemTimer);
            _buildItemTimer = null;
        }
    }

    private void CheckRenderUrl(OfflineRenderListObj[] renderList)
    {
        for(int i = 0; i < renderList.Length; i++)
        {
            var render = renderList[i];
            var abList = render.abList;
            for (int j = 0; j < abList.Length; j++)
            {
                var url = abList[i].renderUrl;
                abList[i].renderUrl = ReplaceRenderUrl(url);
            }
        }
    }

    private string ReplaceRenderUrl(string url)
    {

#if UNITY_ANDROID
        if (url.Contains("ios_buddy_render"))
        {
            url = url.Replace("ios_buddy_render", "android_buddy_render");
        }
#else
        if (url.Contains("android_buddy_render"))
        {
            url = url.Replace("android_buddy_render", "ios_buddy_render");
        }
#endif
        return url;
    }
}
