using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SavingData;
using System.Linq;
using System;

public class CrystalStoneRewardPanel : BasePanel<CrystalStoneRewardPanel>
{
    [SerializeField] private Image rewardImg;
    [SerializeField] private ParticleSystem[] rewardEffect;
    [SerializeField] private GameObject Effect;
    [SerializeField] private Text titleTxt;
    [SerializeField] private Button claimBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private GameObject loading;
    public List<AirdropReward> airdropRewards = new List<AirdropReward>();

    private bool lockState;
    private Tweener scaleTween;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        closeBtn.onClick.AddListener(OnCloseBtnClick);
        claimBtn.onClick.AddListener(OnClaimBtnClick);
        SetEffectVisible(false);
        SetClaimBtnState(false);
    }
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        //禁用JoyStick操作
        lockState = InputReceiver.locked;
        InputReceiver.locked = true;
    }
    public override void OnBackPressed()
    {
        base.OnBackPressed();
        //复原JoyStick操作
        InputReceiver.locked = lockState;
        SetEffectVisible(false);
    }
    /// <summary>
    /// 播放冰晶奖励特效动画
    /// </summary>
    /// <param name="rewardSp"></param>
    public void PlayRewardAnimation(Sprite rewardSp)
    {
        SetEffectVisible(true);
        for (int i = 0; i < rewardEffect.Length; i++)
        {
            rewardEffect[i].Play();
        }
        rewardImg.sprite = rewardSp;
        rewardImg.SetNativeSize();
        rewardImg.transform.localScale = Vector2.zero;
        scaleTween = rewardImg.transform.DOScale(Vector2.one, 0.2f).Play();
    }
    private void SetEffectVisible(bool isVisiable)
    {
        Effect.gameObject.SetActive(isVisiable);
        rewardImg.gameObject.SetActive(isVisiable);
    }
    /// <summary>
    /// 设置按钮状态
    /// /// </summary>
    private void SetClaimBtnState(bool isCanClick)
    {
        claimBtn.GetComponentInChildren<Text>().color = isCanClick ? new Color32(0, 0, 0, 255) : new Color32(158, 158, 158, 255);
        claimBtn.interactable = isCanClick;
        loading.SetActive(!isCanClick);
    }
    private void OnClaimBtnClick()
    {
        MobileInterface.Instance.AddClientRespose(MobileInterface.checkWallet, CheckWalletCallBack);//走端上查询钱包
        MobileInterface.Instance.MobileSendMsgBridge(MobileInterface.checkWallet, "");
    }

    /// <summary>
    /// 查询是钱包回调
    /// </summary>
    /// <param name="response"></param>
    private void CheckWalletCallBack(string response)
    {
        MobileInterface.Instance.DelClientResponse(MobileInterface.checkWallet);
        var jobj = JsonConvert.DeserializeObject<JObject>(response);
        int r = jobj.Value<int>("result");
        if (r == 0) //没绑钱包 先去绑
        {
            UIControlManager.Inst.CallUIControl("snow_create_wallet_enter");
            CreateWalletPanel.Instance.SetAction(CreateWalletSucAct, ExitWalletPanelAct);
        }
        else
        {
            HttpUtils.tokenInfo.walletAddress = jobj.Value<string>("walletAddress");
            ClaimReward();
        }
    }
    /// <summary>
    /// 绑定钱包成功回调
    /// </summary>
    private void CreateWalletSucAct()
    {
        ClaimReward();
    }
    /// <summary>
    /// 退出绑定钱包界面回调
    /// </summary>
    private void ExitWalletPanelAct()
    {
        UIControlManager.Inst.CallUIControl("snow_create_wallet_exit");
    }

    public void GetRewardDatas(Action sucAct)
    {
        JObject jo = new JObject
        {
            ["downtownId"] = GameManager.Inst.gameMapInfo.mapId,
        };
        HttpUtils.MakeHttpRequest("/downtown/airdropRewards", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(jo), (content) =>
       {
           HttpResponDataStruct repData = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
           if (string.IsNullOrEmpty(repData.data))
           {
               LoggerUtils.LogError("GetRewardDatas--->repData.data == null");
               return;
           }
           LoggerUtils.Log("GetRewardDatas--->repData.data:" + content);
           AirdropRewards allRewards = JsonConvert.DeserializeObject<AirdropRewards>(repData.data);
           if (allRewards.airdropRewards.Count > 0)
           {
               this.airdropRewards = allRewards.airdropRewards.FindAll(x => x.canClaim == true);
               sucAct?.Invoke();
           }
       }, (error) =>
       {
           LoggerUtils.LogError("GetRewardDatas--->Fail");
           TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
       });
    }
    public void RemoveReward()
    {
        airdropRewards.RemoveAt(0);
    }
    public AirdropReward GetCurReward()
    {
        if (isCanClaim())
        {
            return airdropRewards.First();
        }
        return null;
    }
    public bool isCanClaim()
    {
        return airdropRewards.Count > 0;
    }
    /// <summary>
    /// 展示奖励物品
    /// </summary>
    public void OpenRewardItem()
    {
        if (!isCanClaim())
        {
            GetRewardDatas(ShowRewardItem);
        }
        else
        {
            ShowRewardItem();
        }
    }
    private void ShowRewardItem()
    {
        var firstRewad = GetCurReward();
        titleTxt.text = firstRewad.rewardName;
        if (!string.IsNullOrEmpty(firstRewad.coverUrl))
        {
            UGCResourcePool.Inst.DownloadAndGet(firstRewad.coverUrl, tex =>
           {
               if (tex != null)
               {
                   Sprite sprite = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                   AKSoundManager.Inst.PostEvent("Play_GreatSnowfield_Ice_Gem_UI_GetRewards", rewardImg.gameObject);
                   PlayRewardAnimation(sprite);
                   SetClaimBtnState(true);
               }
           });
        }
    }
    /// <summary>
    /// 下一个奖励物品
    /// </summary>
    public void ShowNextReward()
    {
        RemoveReward();//移除上一个已领取的
        if (isCanClaim())
        {
            SetClaimBtnState(false);
            SetEffectVisible(false);
            ShowRewardItem();
        }
        else
        {
            HidePanel();
            PlayModePanel.Instance.HideGreatSnowfieldPanel();//隐藏礼物icon
        }
    }

    /// <summary>
    /// 关闭按钮
    /// </summary>
    private void OnCloseBtnClick()
    {
        HidePanel();
    }

    private void HidePanel()
    {
        UIControlManager.Inst.CallUIControl("snowfield_reward_exit");
    }
    /// <summary>
    /// 领取奖励
    /// </summary>
    public void ClaimReward()
    {
        var curClaimReward = GetCurReward();
        if (curClaimReward == null)
        {
            return;
        }
        if (GetRewardClaimStatus(curClaimReward))//被领取完
        {
            TipPanel.ShowToast("Rewards have been claimed, new rewards are on the way, please try again later");
        }
        else
        {
            BatchClaimData bcData = new BatchClaimData
            {
                data = curClaimReward.itemList.Select(x =>
                {
                    return new BatchClaim { budActId = x.budActId, itemId = x.itemId, supply = x.supply };
                }).ToList()
            };
            HttpUtils.MakeHttpRequest("/web3/airdrop/batchClaim", (int)HTTP_METHOD.POST, JsonConvert.SerializeObject(bcData), null, null);
            //展示下一个物品
            ShowNextReward();
        }
    }
    /// <summary>
    /// 获取当前奖励物品是否被领取完(领取状态 0 未领取 1 领取中 2 已领取 3 已领完)
    /// </summary>
    /// <returns></returns>
    private bool GetRewardClaimStatus(AirdropReward airdropReward)
    {
        int status = 0;
        if (airdropReward.itemList == null)
        {
            return true;
        }
        foreach (var item in airdropReward.itemList)
        {
            JObject jo = new JObject
            {
                ["itemId"] = item.itemId,
                ["budActId"] = item.budActId,
            };
            HttpUtils.MakeHttpRequest("/dcTx/airdropClaimStatus", (int)HTTP_METHOD.GET, JsonConvert.SerializeObject(jo), (content) =>
            {
                Debug.LogError("GetRewardClaimStatus:" + content);
                HttpResponDataStruct hResponse = JsonConvert.DeserializeObject<HttpResponDataStruct>(content);
                var jobj = JsonConvert.DeserializeObject<JObject>(hResponse.data);
                status = jobj.Value<int>("status");
            }, (error) =>
            {
                LoggerUtils.LogError("GetAirdropClaimStatus--->Fail:" + item.itemId);
            });
            if (status != 3)//未被领完(有一个未领完则可以重新领)
            {
                return false;
            }
        }
        return true;
    }
    private void CleanTween()
    {
        if (scaleTween != null)
        {
            scaleTween.Kill();
            scaleTween = null;
        }
    }
    private void OnDisable()
    {
        CleanTween();
    }
}
