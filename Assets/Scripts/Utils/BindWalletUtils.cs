using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public enum BindWalletResponseStatus
{
    Fail = 0,
    Success = 1,
    Cancel = 2
}

public class BindWalletUtils 
{
    public static void CheckWalletResponse(string response, Action onBind)
    {
        MobileInterface.Instance.DelClientResponse(MobileInterface.checkWallet);
        var jobj = JsonConvert.DeserializeObject<JObject>(response);
        int r = jobj.Value<int>("result");
        if (r == 0) //没绑钱包 先去绑
        {
            JObject jo = new JObject
            {
                ["method"] = 8
            };
            MobileInterface.Instance.AddClientRespose(MobileInterface.createOrImportWallet, (response)=> OnBindWalletResponse(response, onBind));
            MobileInterface.Instance.MobileSendMsgBridge(MobileInterface.createOrImportWallet, JsonConvert.SerializeObject(jo));
        }
        else
        {
            HttpUtils.tokenInfo.walletAddress = jobj.Value<string>("walletAddress");
            onBind?.Invoke();
        }
    }

    public static void OnBindWalletResponse(string response, Action onBind, Action OnFail = null)
    {
        var jobj = JsonConvert.DeserializeObject<JObject>(response);
        var r = jobj.Value<int>("isSuccess");
        if (r == (int)BindWalletResponseStatus.Success)
        {
            HttpUtils.tokenInfo.walletAddress = jobj.Value<string>("walletAddress");
            onBind?.Invoke();
        }
        else
        {
            OnFail?.Invoke();
            LoggerUtils.Log("RoleUIManager::OnBindWalletResponse wallet not bind");
        }
    }
}
