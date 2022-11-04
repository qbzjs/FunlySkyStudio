using System;
using UnityEngine;

public class PlayerFishingController : MonoBehaviour
{
    internal enum HookPosAdjustType
    {
        LocalPos,
        WorldPos
    }

    public Item Item { get; private set; }
    public int Code { get; private set; }
    public Vector3 HookDropOffset { get; private set; }
    public Vector3 FishParentPos { get { return _rod.GetHookWorldPos(); } }
    public FishingState State { get { return _state; } set { _state = value; _animtionCtrl.isFishing = value != FishingState.None; } }
    private bool IsMyself { get { return _playerId == GameManager.Inst.ugcUserInfo.uid; } }

    private const float HOOK_DROP_SPEED = 3f;
    private const float HOOK_DROP_MAX_DISTANCE = 10f;

    private FishingState _state;
    private string _playerId;

    private RoleController _roleCtrl;
    private AnimationController _animtionCtrl;
    private BudTimer _animationTimer;
    private FishingBehaviour _rod;
    private Animator _rodAnimator;
    private Vector3 _hookFixedOffset;
    private Transform _fishParent;
    private GameObject[] _fishingEffects = new GameObject[4];

    private Transform _hookParent;
    private Transform _hookTrans;
    private HookPosAdjustType _hookPosAdjustType;
    private bool _hookAdjusting = false;
    private float _hookAdjustingTime;
    private float _hookAdjustingTotalTime;
    private Vector3 _hookAdjustSrcPos;
    private Vector3 _hookAdjustDstPos;
    private Action _onHookAdjustCompleted;

    private void Awake()
    {
        _roleCtrl = GetComponentInChildren<RoleController>(true);
        _animtionCtrl = GetComponent<AnimationController>();
    }

    private void OnDestroy()
    {
        FishingCompleted();
    }

    // 初始化
    public void Init(string playerId)
    {
        _playerId = playerId;
    }

    // 设置鱼竿节点
    public void SetFishingRod(FishingBehaviour rod)
    {
        // 设置状态
        State = FishingState.None;

        _rod = rod;
        //_rod.localPosition = Vector3.zero;
        _rod.transform.localRotation = Quaternion.identity;
        _rod.transform.parent.localPosition = Vector3.zero;
        _rod.transform.parent.localRotation = Quaternion.identity;

        // 获取鱼竿动画播放器
        _rodAnimator = _rod.GetComponent<Animator>();

        // 由于鱼漂是动态设置到鱼竿上的，所有刷新一下骨骼节点
        _rodAnimator.Rebind();

        // 获取鱼钩父节点
        _hookParent = _rod.transform.Find("root_fish1/FishingHookParent");
        _hookTrans = _hookParent.GetChild(0);
        _fishParent = _rod.transform.Find("root_fish1/FishingHookParent/FishingHook/root_fish3");
        for (int i = 0; i < 4; i++)
        {
            string effectName = string.Format("root_fish1/FishingHookParent/FishingHook/YUBIAO1_Mesh/FishingEffect_0{0}", i + 1);
            _fishingEffects[i] = _rod.transform.Find(effectName).gameObject;
        }
    }

    public Vector3 PreCalcHookDropOffset()
    {
        // 射线检测浮标下方的碰撞体 
        var raycastLayerMask = ~LayerMask.GetMask("PostProcess", "OtherPlayer", "PVPArea", "Anchors", "Head", "Weapon", "TriggerModel", "Trigger", "Airwall");
        RaycastHit raycastHit;
        var offset = Physics.Raycast(_roleCtrl.transform.position + _roleCtrl.transform.forward * 5f, Vector3.down, out raycastHit, HOOK_DROP_MAX_DISTANCE, raycastLayerMask, QueryTriggerInteraction.Collide)
            ? Vector3.down * (raycastHit.distance)
            : Vector3.down * HOOK_DROP_MAX_DISTANCE;

        return offset;
    }

    // 开始钓鱼
    public void StartFishing(Vector3 offset)
    {
        // 设置状态
        State = FishingState.Fishing;
        Code = FishingCode.FISHING_FAILED_NO_FISH;

        HookDropOffset = offset;

        _rodAnimator.enabled = true;

        // 播放抛竿动画
        PlayPlayerAnimation("FishingStart");
        PlayerRodAnimation("FishingStart");

        // 播放钓鱼音效
        PlayFishingAudio("Play_Fishing_Throw_Rod");

        StartTimer(1.63f, () => {
            if (IsMyself)
            {
                MessageHelper.Broadcast(MessageName.OnFishingStart);
                PlayerBaseControl.Inst.Move(Vector3.zero);
            }
            // 浮漂以固定速度掉落
            var srcPos = _hookParent.position;
            var dstPos = _hookParent.position + offset;
            StartAdjustHookPos(HookPosAdjustType.WorldPos, srcPos, dstPos, Vector3.Distance(srcPos, dstPos) / HOOK_DROP_SPEED, () => {
                // 播放浮标掉落水里的特效
                PlayFishingEffect(1);

                StartTimer(0.5f, () => {
                    // 播放浮标浮在水面上的特效
                    PlayFishingEffect(3);
                });
            });
        });
    }

    // 立刻进入钓鱼中
    public void StartFishingImmediate(Vector3 offset)
    {
        State = FishingState.Fishing;

        _rodAnimator.enabled = true;
        PlayPlayerAnimation("FishingIdle");
        PlayerRodAnimation("FishingIdle");

        StartTimer(0.1f, () => {
            StartAdjustHookPos(HookPosAdjustType.WorldPos, _hookParent.position, _hookParent.position + offset, 0.1f);

            // 播放浮标浮在水面上的特效
            PlayFishingEffect(3);
        });
        if (IsMyself)
        {
            MessageHelper.Broadcast(MessageName.OnFishingStart);
        }
    }

    // 收竿
    public void PullFishingRod(int code, Item item)
    {
        Code = code;
        Item = item;

        switch (code)
        {
            case FishingCode.FISHING_SUCCESS:
                State = FishingState.FishingSuccess;
                PullRodWithFish(item);
                break;
            case FishingCode.FISHING_FAILED_BAG_FULL:
            case FishingCode.FISHING_FAILED_NO_BAG:
                State = FishingState.FishingFailed;
                PullRodWithFish(item);
                break;
            case FishingCode.FISHING_FAILED_NO_FISH:
            case FishingCode.FISHING_FAILED_CONFLICT_FISH:
                State = FishingState.FishingFailed;
                PullRodWithoutFish();
                break;
        }
    }

    // 立刻收完竿
    public void PullFishingRodImmediate(int code, Item item)
    {
        Code = code;
        Item = item;

        _rodAnimator.enabled = true;
        switch (code)
        {
            case FishingCode.FISHING_SUCCESS:
                State = FishingState.FishingSuccess;
                PutFishOnHook(item);
                PlayPlayerAnimation("FishingShowFish");
                PlayerRodAnimation("FishingShowFish");
                break;
            case FishingCode.FISHING_FAILED_BAG_FULL:
            case FishingCode.FISHING_FAILED_NO_BAG:
                State = FishingState.FishingFailed;
                PutFishOnHook(item);
                PlayPlayerAnimation("FishingShowFish");
                PlayerRodAnimation("FishingShowFish");
                break;
            case FishingCode.FISHING_FAILED_NO_FISH:
            case FishingCode.FISHING_FAILED_CONFLICT_FISH:
                State = FishingState.FishingFailed;
                PlayPlayerAnimation("FishingIdle");
                PlayerRodAnimation("FishingIdle");

                // 如果是我自己，自动发送结束钓鱼指令
                if (IsMyself)
                    FishingManager.Inst.StopFishing();
                break;
        }
    }

    // 收竿，有鱼
    private void PullRodWithFish(Item item)
    {
        // 播放有鱼收竿的音效
        PlayFishingAudio("Play_Fishing_Pull_Capture");

        // 播放有鱼收竿动画
        PlayPlayerAnimation("FishingPullRodWithFish");
        PlayerRodAnimation("FishingPullRodWithFish");

        // 把鱼挂在鱼钩上
        PutFishOnHook(item);

        // 播放鱼儿上钩的特效
        PlayFishingEffect(0);

        // 把鱼钩先固定住不动
        StartAdjustHookPos(HookPosAdjustType.WorldPos, _hookParent.position, _hookParent.position, 2.83f);
        StartTimer(2.83f, () => {
            // 把鱼钩拉上来
            StartAdjustHookPos(HookPosAdjustType.LocalPos, _hookParent.localPosition, Vector3.zero, 0.13f);

            if(IsMyself && FishingCtrPanel.Instance)
            {
                FishingCtrPanel.Instance.SetZoom();
            }

            StartTimer(2.38f, () => {
                // 进入展示鱼状态，此处动画会自动切换
                State = FishingState.ShowFish;
                if (IsMyself)
                {
                    if (FishingCtrPanel.Instance)
                    {
                        FishingCtrPanel.Instance.EnterShowFishMode(true);
                    }
                }
            });
        });
    }

    // 收竿，没有鱼
    private void PullRodWithoutFish()
    {
        // 播放无鱼收竿的音效
        PlayFishingAudio("Play_Fishing_Pull_No_Capture");

        // 播放无鱼收竿动画
        PlayPlayerAnimation("FishingPullRodNoFish");
        PlayerRodAnimation("FishingPullRodNoFish");

        // 播放鱼钩出水的特效
        PlayFishingEffect(2);

        // 把鱼钩拉上来
        StartAdjustHookPos(HookPosAdjustType.LocalPos, _hookParent.localPosition, Vector3.zero, 0.1f);
        StartTimer(1.0f, () => {
            State = FishingState.ShowEmpty;

            // 如果是我自己，自动发送结束钓鱼指令
            if (IsMyself)
                FishingManager.Inst.StopFishing();
        });
    }

    // 结束钓鱼
    public void StopFishing(int code, Item item)
    {
        if (IsMyself)
        {
            MessageHelper.Broadcast(MessageName.OnFishingStop);
        }
        switch (code)
        {
            case FishingCode.FISHING_SUCCESS:
                StopFishing_GetFish(item);
                break;
            case FishingCode.FISHING_FAILED_BAG_FULL:
            case FishingCode.FISHING_FAILED_NO_BAG:
                StopFishing_ThrowFish(item);
                break;
            case FishingCode.FISHING_FAILED_NO_FISH:
            case FishingCode.FISHING_FAILED_CONFLICT_FISH:
                FishingCompleted();
                break;
        }
    }

    // 结束钓鱼，把鱼放到背包里
    private void StopFishing_GetFish(Item item)
    {
        // 播放把鱼放到背包的音效
        PlayFishingAudio("Play_Fishing_Pickup");

        // 播放把鱼放进背包的动画
        PlayPlayerAnimation("FishingStopAndGetFish");
        PlayerRodAnimation("FishingStopAndGetFish");

        // 等待动画播放完成，移除鱼
        StartTimer(1.0f, () => {
            if (Item != null)
            {
                var fish = FishingManager.Inst.GetFish(item.id);
                if (fish != null)
                {
                    fish.gameObject.SetActive(false);

                    var entity = fish.entity;
                    var pickComp = entity.Get<PickablityComponent>();
                    var gComp = entity.Get<GameObjectComponent>();
                    var uid = gComp.uid;
                    pickComp.isPicked = true;
                    pickComp.alreadyPicked = true;
                    PickabilityManager.Inst.RemoveMovementComp(fish);
                    PickabilityManager.Inst.SetComponentEnable(fish.gameObject, false);

                    if (SceneParser.Inst.GetBaggageSet() == 1)
                    {
                        PickabilityManager.Inst.RecordPlayerPick(_playerId, uid);
                        BaggageManager.Inst.AddNewItem(_playerId, uid, gComp.resId);
                        var fishParent = FishingManager.Inst.GetFishParent(uid);
                        if (!PickabilityManager.Inst.PropParentDic.ContainsKey(uid) && fishParent != null)
                        {
                            PickabilityManager.Inst.PropParentDic.Add(uid, fishParent);
                        }

                        var pickNode = _roleCtrl.GetBandNode((int)BodyNode.PickNode);
                        if (pickNode != null)
                        {
                            fish.transform.SetParent(pickNode);
                            fish.transform.localPosition = Vector3.zero;
                            fish.transform.localEulerAngles = PickabilityManager.Inst.GetOriQuaternion(uid);
                            fish.transform.position = fish.transform.TransformPoint(-pickComp.anchors);
                        }
                    }
                }
            }

            // 钓鱼结束
            FishingCompleted();
        });
    }

    // 结束钓鱼，把鱼丢回场景上
    private void StopFishing_ThrowFish(Item item)
    {
        // 播放把鱼丢到水里的音效
        PlayFishingAudio("Play_Fishing_Discard");

        // 播放把鱼丢到水里的动画
        PlayPlayerAnimation("FishingStopAndThrowFish");
        PlayerRodAnimation("FishingStopAndThrowFish");

        StartTimer(1.167f, () => {
            // 把鱼从鱼钩上放回场景，钓鱼结束
            PushBackFromHook(item);
            FishingCompleted();
        });
    }

    // 把鱼挂在鱼钩上
    private void PutFishOnHook(Item item)
    {
        if (item == null)
            return;

        var fish = FishingManager.Inst.GetFish(item.id);
        if (fish != null)
        {
            fish.transform.SetParent(_fishParent);
            fish.transform.localPosition = Vector3.zero;
        }
    }

    // 把鱼从鱼钩上放回场景
    private void PushBackFromHook(Item item)
    {
        if (item == null)
            return;

        //若开启背包并且鱼已经被收到背包中，则不做处理。否则丢到场景中
        var fish = FishingManager.Inst.GetFish(item.id);
        if (fish != null) {
            if (SceneParser.Inst.GetBaggageSet() != 1 || (BaggageManager.Inst != null && BaggageManager.Inst.playerBaggageDic.ContainsKey(_playerId) && !BaggageManager.Inst.playerBaggageDic[_playerId].Contains(item.id)))
            {
                fish.transform.SetParent(FishingManager.Inst.GetFishParent(item.id));
            }
            PickabilityManager.Inst.RemoveMovementComp(fish);
        }
    }

    // 钓鱼整个流程结束
    private void FishingCompleted()
    {
        State = FishingState.None;

        StopTimer();
        StopAdjustHookPos();
        HideAllFishingEffect();
        PushBackFromHook(Item);
        PlayPlayerAnimation("idle");
        _rodAnimator.enabled = false;
    }

    private void Update()
    {
        if (_hookAdjusting == true)
        {
            _hookAdjustingTime += Time.deltaTime;
            var interpolation = _hookAdjustingTotalTime > 0 ? _hookAdjustingTime / _hookAdjustingTotalTime : 1.0f; // 避免除零
            var hookPos = Vector3.Lerp(_hookAdjustSrcPos, _hookAdjustDstPos, interpolation);
            if (_hookPosAdjustType == HookPosAdjustType.WorldPos)
                _hookParent.position = hookPos;
            else if(_hookPosAdjustType == HookPosAdjustType.LocalPos)
                _hookParent.localPosition = hookPos;

            if (_hookAdjustingTime >= _hookAdjustingTotalTime)
                StopAdjustHookPos();
        }
    }

    private void StartAdjustHookPos(HookPosAdjustType type, Vector3 srcPos, Vector3 dstPos, float time, Action callback = null)
    {
        _hookPosAdjustType = type;
        _hookAdjustSrcPos = srcPos;
        _hookAdjustDstPos = dstPos;
        _hookAdjustingTotalTime = time;
        _onHookAdjustCompleted = callback;

        _hookAdjustingTime = 0.0f;
        _hookAdjusting = true;
    }

    private void StopAdjustHookPos()
    {
        _onHookAdjustCompleted?.Invoke();
        _onHookAdjustCompleted = null;

        _hookAdjusting = false;
        _hookAdjustingTime = 0.0f;
        _hookAdjustingTotalTime = 0;
    }

    private void PlayFishingEffect(int index)
    {
        if (index < 0 || index >= _fishingEffects.Length)
            return;

        HideAllFishingEffect();

        var effectTrans = _fishingEffects[index].transform;
        effectTrans.gameObject.SetActive(true);

        var hookPos = _hookParent.GetChild(0).position;
        switch (index)
        {
            case 0:
                effectTrans.localPosition = new Vector3(-0.003060047f, -0.01087004f, -0.0005799826f);
                effectTrans.localEulerAngles = new Vector3(-105.927f, -100.499f, -4.330017f);
                break;
            case 1:
                effectTrans.localPosition = new Vector3(0.0004686857f, -0.0001246921f, -9.983693e-05f);
                effectTrans.localEulerAngles = new Vector3(-38.348f, 88.79f, -90.358f);
                break;
            case 2:
                effectTrans.localPosition = new Vector3(0.0004686841f, -0.0001246855f, -9.983969e-05f);
                effectTrans.localEulerAngles = new Vector3(-38.476f, 89.104f, -90.399f);
                break;
            case 3:
                effectTrans.localPosition = new Vector3(0.0004686907f, -0.000124688f, -9.983586e-05f);
                effectTrans.localEulerAngles = new Vector3(-38.372f, 88.855f, -90.379f);
                break;
        }

        _rod.EnableHookEffet(index == 3);
    }

    private void HideAllFishingEffect()
    {
        for (int i = 0; i < _fishingEffects.Length; ++i)
            _fishingEffects[i].SetActive(false);

        _rod.EnableHookEffet(false);
    }

    private void StartTimer(float time, Action callback)
    {
        StopTimer();
        _animationTimer = TimerManager.Inst.RunOnce("FishingTimer", time, callback);
    }

    private void StopTimer()
    {
        if (_animationTimer != null && _animationTimer.IsDisposed == false)
            TimerManager.Inst.Stop(_animationTimer);
    }

    private void PlayPlayerAnimation(string name)
    {
        if (_animtionCtrl == null)
            return;

        _animtionCtrl.RleasePrefab();
        _animtionCtrl.CancelLastEmo();
        _animtionCtrl.PlayAnim(null, name);
    }

    private void PlayerRodAnimation(string name)
    {
        if (_rodAnimator.enabled == false)
            _rodAnimator.enabled = true;

        _rodAnimator.Play(name);
    }

    private void PlayFishingAudio(string audioName)
    {
        AKSoundManager.Inst.PlayAttackSound("", audioName, "", gameObject);
    }

    public void StopFishingAudio()
    {
        var playerObj = this.gameObject;
        AKSoundManager.Inst.PlayAttackSound("", "Stop_Fishing_Throw_Rod", "", playerObj);
        AKSoundManager.Inst.PlayAttackSound("", "Stop_Fishing_Pull_No_Capture", "", playerObj);
        AKSoundManager.Inst.PlayAttackSound("", "Stop_Fishing_Pull_Capture", "", playerObj);
        AKSoundManager.Inst.PlayAttackSound("", "Stop_Fishing_Discard", "", playerObj);
        AKSoundManager.Inst.PlayAttackSound("", "Stop_Fishing_Pickup", "", playerObj);
    }
}
