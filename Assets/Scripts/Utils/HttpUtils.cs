/// <summary>
/// Author:Mingo-LiZongMing
/// Description:Http请求工具
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BestHTTP;
using BestHTTP.JSON;
using UnityEngine.Networking;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Net;

public enum RequestMode
{
    SET_MAP = 1,
    GET_MAP = 2,
    SET_PEOPLE_IMAGE = 3,
    GET_PEOPLE_IMAGE = 4,
}

public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}

public class HttpUtils : MonoBehaviour
{
    public static SavingData.UnityBaseInfo tokenInfo = new SavingData.UnityBaseInfo();
    public static string RequestUrl = "";
    public static bool IsMaster = false;
    public static bool IsAlpha = true;
    private const string baseUrl = "https://api.joinbudapp.com";
    private const string alphaUrl = "https://api-alpha.joinbudapp.com";
    private const string masterUrl = "https://api-test.joinbudapp.com";
    public const int HttpTimeout = 10;

    public static void MakeHttpRequest(string _path, int _requestType, string _paramStr, UnityAction<string> onReceive, UnityAction<string> onFail)
    {

        string url = baseUrl;
#if !UNITY_EDITOR
        if (!string.IsNullOrEmpty(RequestUrl))
        {
            url = RequestUrl;
        }
#else
        if (IsMaster)
        {
            url = masterUrl;
        }
        else if(IsAlpha)
        {
            url = alphaUrl;
        }
#endif

        SavingData.HTTPRequest requestData = new SavingData.HTTPRequest
        {
            path = _path,
            requestType = _requestType,
            paramStr = _paramStr
        };

        OnRequestFinishedDelegate callBack = (HTTPRequest req, HTTPResponse resp) =>
        {
            SavingData.HttpResponseRaw responseDataRaw = new SavingData.HttpResponseRaw();

            LoggerUtils.Log(string.Format("HttpRespone callBack request.State:{0} ", req.State));
            if (req.State != HTTPRequestStates.Finished || resp == null)
            {
                responseDataRaw.result = -1;
                responseDataRaw.rmsg = "http error request.State:" + req.State;
                onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
                LoggerUtils.LogError("HttpRequsetError-" + "HttpPath = " + _path + " Rmsg = " + responseDataRaw.rmsg);
                return;
            }
            if (!resp.IsSuccess)
            {
                string reason = string.Format("http error:StatusCode: {0},  Message:{1},  data: {2}", resp.StatusCode, resp.Message, resp.DataAsText);
                //LoggerUtils.LogError(reason);
                responseDataRaw.result = -1;
                responseDataRaw.rmsg = reason;
                LoggerUtils.LogError("HttpRequsetError-" + "HttpPath = " + _path + " Rmsg = " + reason);
                onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
                return;
            }
            LoggerUtils.Log(string.Format("http callBack:StatusCode: {0},  Message:{1},  data: {2}", resp.StatusCode, resp.Message, resp.DataAsText));
            responseDataRaw = JsonConvert.DeserializeObject<SavingData.HttpResponseRaw>(resp.DataAsText);
            if (responseDataRaw.result != 0)
            {
                LoggerUtils.LogError("HttpRequsetError-" + "HttpPath = " + _path + " Rmsg = " + resp.DataAsText);
                onFail?.Invoke(resp.DataAsText);
                return;
            }
            SavingData.HttpResponse responseData = new SavingData.HttpResponse
            {
                identifier = "",
                isSuccess = 0 //1 success
            };
            responseData.data = JsonConvert.SerializeObject(responseDataRaw.data);
            responseData.isSuccess = 1;
            string resultRspSuccess = JsonConvert.SerializeObject(responseData);
            onReceive?.Invoke(resultRspSuccess);
        };

        HTTPRequest request;
        if (_requestType == (int)HTTP_METHOD.POST)
        {
            url += requestData.path;
            request = new HTTPRequest(new Uri(url), HTTPMethods.Post, callBack);
            request.RawData = Encoding.UTF8.GetBytes(_paramStr);
            request.Timeout = TimeSpan.FromSeconds(HttpTimeout);
        }
        else
        {
            if (!string.IsNullOrEmpty(requestData.paramStr))
            {
                JObject jObject = JObject.Parse(requestData.paramStr);
                IEnumerable<string> nameValues = jObject
                    .Properties()
                    .Select(x => $"{x.Name}={x.Value}");
                url += requestData.path + "?" + string.Join("&", nameValues);
            }
            else
            {
                url += requestData.path;
            }
            request = new HTTPRequest(new Uri(url), HTTPMethods.Get, callBack);
            request.Timeout = TimeSpan.FromSeconds(HttpTimeout);
        }
        LoggerUtils.Log(string.Format("HttpRequest url:{0} \n paramStr:{1}", url, requestData.paramStr));

#if !UNITY_EDITOR

        LoggerUtils.Log("tokenInfo = " + JsonConvert.SerializeObject(tokenInfo));
        request.AddHeader("uid", tokenInfo.uid);
        request.AddHeader("baseUrl", tokenInfo.baseUrl);
        request.AddHeader("environment", tokenInfo.environment);
        request.AddHeader("device", tokenInfo.device);
        request.AddHeader("platform", "U3D");
        request.AddHeader("generation", tokenInfo.generation);
        request.AddHeader("token", tokenInfo.token);
        request.AddHeader("locale", tokenInfo.locale);
        request.AddHeader("lang", tokenInfo.lang);
        request.AddHeader("version", GameManager.Inst.unityConfigInfo.appVersion);
        request.AddHeader("walletAddress", tokenInfo.walletAddress);
        request.AddHeader("timezone", tokenInfo.timezone);
        request.AddHeader("Content-Type", "application/json");
        string newUser = "";
        if ((SCENE_TYPE)GameManager.Inst.engineEntry.sceneType == SCENE_TYPE.ROLE_SCENE
        && (ROLE_TYPE)GameManager.Inst.engineEntry.subType == ROLE_TYPE.FIRST_ENTRY)
        {
            newUser = "new";
        }
        request.AddHeader("userRole", newUser);
#else
        request.AddHeader("uid", TestNetParams.testHeader.uid);
        request.AddHeader("baseUrl", TestNetParams.testHeader.baseUrl);
        request.AddHeader("environment", TestNetParams.testHeader.environment);
        request.AddHeader("device", TestNetParams.testHeader.device);
        request.AddHeader("platform", TestNetParams.testHeader.platform);
        request.AddHeader("generation", TestNetParams.testHeader.generation);
        request.AddHeader("token", TestNetParams.testHeader.token);
        request.AddHeader("locale", TestNetParams.testHeader.locale);
        request.AddHeader("lang", TestNetParams.testHeader.lang);
        request.AddHeader("version", TestNetParams.testHeader.version);
        request.AddHeader("walletAddress", TestNetParams.testHeader.walletAddress);
        request.AddHeader("timezone", DataUtils.GetLocalTimeZone());
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("userRole", "");
        LoggerUtils.Log("testHeader:" + JsonConvert.SerializeObject(TestNetParams.testHeader));
#endif
        if (Application.platform == RuntimePlatform.Android)
        {
            request.AddHeader("mobile", "android");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            request.AddHeader("mobile", "ios");
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            // 编辑器状态下默认视为android
            request.AddHeader("mobile", "android");
        }
        request.Send();
    }

    public static void Release()
    {
        HTTPManager.OnQuit();
    }

    public static void MakeUnityRequest(string _path, int _requestType, string _paramStr, UnityAction<string> onReceive, UnityAction<string> onFail)
    {
        string url = CombineUrl(_path,_requestType,_paramStr);
        CoroutineManager.Inst.StartCoroutine(HttpUtils.RequestUnitySync(url, _requestType, _paramStr, onReceive, onFail));
    }

    public static IEnumerator RequestUnitySync(string url, int _requestType, string _paramStr, UnityAction<string> onReceive, UnityAction<string> onFail)
    {
        if (string.IsNullOrEmpty(url))
        {
            yield break;
        }

        UnityWebRequest webRequest = null;
        if (_requestType == (int)HTTP_METHOD.POST)
        {
            webRequest = UnityWebRequest.Post(url, _paramStr);
        }
        else
        {
            webRequest = UnityWebRequest.Get(url);
        }

        webRequest.timeout = HttpTimeout;

        InitHttpHeader(webRequest);
        webRequest.certificateHandler =new BypassCertificate() ;

        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            LoggerUtils.Log("RequestUnitySync error：" + webRequest.error);
            SavingData.HttpResponseRaw responseDataRaw = new SavingData.HttpResponseRaw();
            responseDataRaw.result = -1;
            responseDataRaw.rmsg = webRequest.error;
            onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
        }
        else
        {
            // onReceive.Invoke(webRequest.downloadHandler.text);
            LoggerUtils.Log("RequestUnitySync rsp content：" + webRequest.downloadHandler.text);
            SavingData.HttpResponseRaw responseDataRaw = JsonConvert.DeserializeObject<SavingData.HttpResponseRaw>(webRequest.downloadHandler.text);
            if (responseDataRaw.result != 0)
            {
                LoggerUtils.Log("RequestUnitySync server result:"+responseDataRaw.result + "  content:"+webRequest.downloadHandler.text);
                onFail?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                SavingData.HttpResponse responseData = new SavingData.HttpResponse
                {
                    identifier = "",
                    isSuccess = 0 //1 success
                };
                responseData.data = JsonConvert.SerializeObject(responseDataRaw.data);
                responseData.isSuccess = 1;
                string resultRspSuccess = JsonConvert.SerializeObject(responseData);
                onReceive?.Invoke(resultRspSuccess);
            }
        }
    }

    
    public static void MakeAsyncRequest(string _path, int _requestType, string _paramStr, UnityAction<string> onReceive, UnityAction<string> onFail)
    {
        string url = CombineUrl(_path,_requestType,_paramStr);
        LoggerUtils.Log("##MakeAsyncRequest path:"+url + "   param:"+_paramStr);
        HTTPAsyncHelper.GetHTTPAsync(url,_requestType,_paramStr,(HttpPack httpPack)=>{
            LoggerUtils.Log("##MakeAsyncRequest返回数据 StatusCode："+httpPack.StatusCode + "   ResponeData:"+httpPack.ResponeData);
            if(httpPack.StatusCode < 200)
            {
                LoggerUtils.Log("##MakeAsyncRequest 请求失败用bestHttp重试");
                MakeHttpRequest(_path,_requestType,_paramStr,onReceive,onFail);
            }
            else if (httpPack.StatusCode != 200)
            {
                LoggerUtils.Log("MakeAsyncRequest error：" + httpPack.StatusCode);
                SavingData.HttpResponseRaw responseDataRaw = new SavingData.HttpResponseRaw();
                responseDataRaw.result = -1;
                responseDataRaw.rmsg = "MakeAsyncRequest error:"+httpPack.StatusCode;
                onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
            }
            else if(string.IsNullOrEmpty(httpPack.ResponeData)){
                SavingData.HttpResponseRaw responseDataRaw = new SavingData.HttpResponseRaw();
                responseDataRaw.result = -1;
                responseDataRaw.rmsg = "MakeAsyncRequest ResponeData is null:"+httpPack.StatusCode;
                onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
            }
            else
            {
                LoggerUtils.Log("MakeAsyncRequest rsp content：" + httpPack.ResponeData);
                SavingData.HttpResponseRaw responseDataRaw = JsonConvert.DeserializeObject<SavingData.HttpResponseRaw>(httpPack.ResponeData);
                if (responseDataRaw.result != 0)
                {
                    LoggerUtils.Log("MakeAsyncRequest server result:" + responseDataRaw.result + "  content:" + httpPack.ResponeData);
                    onFail?.Invoke(httpPack.ResponeData);
                }
                else
                {
                    SavingData.HttpResponse responseData = new SavingData.HttpResponse
                    {
                        identifier = "",
                        isSuccess = 0 //1 success
                    };
                    responseData.data = JsonConvert.SerializeObject(responseDataRaw.data);
                    responseData.isSuccess = 1;
                    string resultRspSuccess = JsonConvert.SerializeObject(responseData);
                    onReceive?.Invoke(resultRspSuccess);
                }
            }
        });
    }

    public static string CombineUrl(string _path, int _requestType, string _paramStr)
    {
        string url = baseUrl;
        if (!string.IsNullOrEmpty(RequestUrl))
        {
            url = RequestUrl;
        }

#if UNITY_EDITOR
        if (IsMaster)
        {
            url = masterUrl;
        }
#endif

        if (_requestType == (int)HTTP_METHOD.GET && !string.IsNullOrEmpty(_paramStr))
        {
            JObject jObject = JObject.Parse(_paramStr);
            IEnumerable<string> nameValues = jObject
                .Properties()
                .Select(x => $"{x.Name}={x.Value}");
            url += _path + "?" + string.Join("&", nameValues);
        }
        else
        {
            url += _path;
        }
        return url;
    }

    //初始化UnityWebRequest header
    public static void InitHttpHeader(UnityWebRequest webRequest)
    {
#if !UNITY_EDITOR
        webRequest.SetRequestHeader("uid", tokenInfo.uid);
        webRequest.SetRequestHeader("baseUrl", tokenInfo.baseUrl);
        webRequest.SetRequestHeader("environment", tokenInfo.environment);
        webRequest.SetRequestHeader("device", tokenInfo.device);
        webRequest.SetRequestHeader("platform", "U3D");
        webRequest.SetRequestHeader("generation", tokenInfo.generation);
        webRequest.SetRequestHeader("token", tokenInfo.token);
        webRequest.SetRequestHeader("locale", tokenInfo.locale);
        webRequest.SetRequestHeader("lang", tokenInfo.lang);
        webRequest.SetRequestHeader("version", GameManager.Inst.unityConfigInfo.appVersion);
        webRequest.SetRequestHeader("walletAddress", tokenInfo.walletAddress);
        webRequest.SetRequestHeader("timezone", tokenInfo.timezone);
#else

        webRequest.SetRequestHeader("uid", TestNetParams.testHeader.uid);
        webRequest.SetRequestHeader("baseUrl", TestNetParams.testHeader.baseUrl);
        webRequest.SetRequestHeader("environment", TestNetParams.testHeader.environment);
        webRequest.SetRequestHeader("device", TestNetParams.testHeader.device);
        webRequest.SetRequestHeader("platform", TestNetParams.testHeader.platform);
        webRequest.SetRequestHeader("generation", TestNetParams.testHeader.generation);
        webRequest.SetRequestHeader("token", TestNetParams.testHeader.token);
        // webRequest.SetRequestHeader("locale", TestNetParams.testHeader.locale);
        // webRequest.SetRequestHeader("lang", TestNetParams.testHeader.lang);
        webRequest.SetRequestHeader("version", TestNetParams.testHeader.version);
        webRequest.SetRequestHeader("walletAddress", TestNetParams.testHeader.walletAddress);
        webRequest.SetRequestHeader("timezone", DataUtils.GetLocalTimeZone());
#endif
        if (Application.platform == RuntimePlatform.Android)
        {
            webRequest.SetRequestHeader("mobile", "android");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            webRequest.SetRequestHeader("mobile", "ios");
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            // 编辑器状态下默认视为android
            webRequest.SetRequestHeader("mobile", "android");
        }
    }


     //初始化HttpWebRequest header
    public static void InitHttpHeader(HttpWebRequest webRequest)
    {
#if !UNITY_EDITOR
        webRequest.Headers.Add("uid", tokenInfo.uid);
        webRequest.Headers.Add("baseUrl", tokenInfo.baseUrl);
        webRequest.Headers.Add("environment", tokenInfo.environment);
        webRequest.Headers.Add("device", tokenInfo.device);
        webRequest.Headers.Add("platform", "U3D");
        webRequest.Headers.Add("generation", tokenInfo.generation);
        webRequest.Headers.Add("token", tokenInfo.token);
        webRequest.Headers.Add("locale", tokenInfo.locale);
        webRequest.Headers.Add("lang", tokenInfo.lang);
        webRequest.Headers.Add("version", GameManager.Inst.unityConfigInfo.appVersion);
        webRequest.Headers.Add("walletAddress", tokenInfo.walletAddress);
        webRequest.Headers.Add("timezone", tokenInfo.timezone);
#else
        webRequest.Headers.Add("uid", TestNetParams.testHeader.uid);
        webRequest.Headers.Add("baseUrl", TestNetParams.testHeader.baseUrl);
        webRequest.Headers.Add("environment", TestNetParams.testHeader.environment);
        webRequest.Headers.Add("device", TestNetParams.testHeader.device);
        webRequest.Headers.Add("platform", TestNetParams.testHeader.platform);
        webRequest.Headers.Add("generation", TestNetParams.testHeader.generation);
        webRequest.Headers.Add("token", TestNetParams.testHeader.token);
        webRequest.Headers.Add("version", TestNetParams.testHeader.version);
        webRequest.Headers.Add("walletAddress", TestNetParams.testHeader.walletAddress);
        webRequest.Headers.Add("timezone", DataUtils.GetLocalTimeZone());
#endif
        if (Application.platform == RuntimePlatform.Android)
        {
            webRequest.Headers.Add("mobile", "android");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            webRequest.Headers.Add("mobile", "ios");
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            // 编辑器状态下默认视为android
            webRequest.Headers.Add("mobile", "android");
        }
    }





    #region 本地测试环境使用
#if UNITY_EDITOR

    /// <summary>
    /// 联机本地测试使用-连本地服获取Session时调用
    /// </summary>
    public static void UnityLocalTest_MakeTestHttpRequest(string _path, int _requestType, string _paramStr, UnityAction<string> onReceive, UnityAction<string> onFail)
    {
        string url = baseUrl;
        url = TestNetParams.Inst.GetMakeHttpUrl();

        SavingData.HTTPRequest requestData = new SavingData.HTTPRequest
        {
            path = _path,
            requestType = _requestType,
            paramStr = _paramStr
        };

        OnRequestFinishedDelegate callBack = (HTTPRequest req, HTTPResponse resp) =>
        {
            if (req != null && resp != null)
            {
                LoggerUtils.Log(string.Format("HttpRespone callBack request.State:{0} \nrespone.IsSuccess:{1} \n respone.data:{2}", req.State, resp.IsSuccess, resp.DataAsText));
            }
            if (req.State != HTTPRequestStates.Finished)
            {
                LoggerUtils.Log("###http 异常："+resp?.Message);
                onFail?.Invoke("http error request.State:" + req.State);
                return;
            }
            if (!resp.IsSuccess)
            {
                string reason = string.Format("http error:StatusCode: {0},  Message:{1},  data: {2}", resp.StatusCode, resp.Message, resp.DataAsText);
                LoggerUtils.Log(reason);
                onFail?.Invoke(reason);
                return;
            }

            SavingData.HttpResponseRaw responseDataRaw = JsonConvert.DeserializeObject<SavingData.HttpResponseRaw>(resp.DataAsText);
            if (responseDataRaw.result != 0)
            {
                onFail?.Invoke(resp.DataAsText);
                return;
            }
            SavingData.HttpResponse responseData = new SavingData.HttpResponse
            {
                identifier = "",
                isSuccess = 0 //1 success
            };
            responseData.data = JsonConvert.SerializeObject(responseDataRaw.data);
            responseData.isSuccess = 1;
            string resultRspSuccess = JsonConvert.SerializeObject(responseData);
            onReceive?.Invoke(resultRspSuccess);

        };

        HTTPRequest request;
        if (_requestType == (int)HTTP_METHOD.POST)
        {
            url += requestData.path;
            request = new HTTPRequest(new Uri(url), HTTPMethods.Post, callBack);
            request.RawData = Encoding.UTF8.GetBytes(_paramStr);
            request.Timeout = TimeSpan.FromSeconds(HttpTimeout);
        }
        else
        {
            if (!string.IsNullOrEmpty(requestData.paramStr))
            {
                JObject jObject = JObject.Parse(requestData.paramStr);
                IEnumerable<string> nameValues = jObject
                    .Properties()
                    .Select(x => $"{x.Name}={x.Value}");
                url += requestData.path + "?" + string.Join("&", nameValues);
            }
            else
            {
                url += requestData.path;
            }
            request = new HTTPRequest(new Uri(url), HTTPMethods.Get, callBack);
            request.Timeout = TimeSpan.FromSeconds(HttpTimeout);
        }
        LoggerUtils.Log(string.Format("HttpRequest url:{0} \n paramStr:{1}", url, requestData.paramStr));

        request.AddHeader("uid", TestNetParams.testHeader.uid);
        request.AddHeader("baseUrl", TestNetParams.testHeader.baseUrl);
        request.AddHeader("environment", TestNetParams.testHeader.environment);
        request.AddHeader("device", TestNetParams.testHeader.device);
        request.AddHeader("platform", TestNetParams.testHeader.platform);
        request.AddHeader("generation", TestNetParams.testHeader.generation);
        request.AddHeader("token", TestNetParams.testHeader.token);
        // request.AddHeader("locale", TestNetParams.testHeader.locale);
        // request.AddHeader("lang", TestNetParams.testHeader.lang);
        request.AddHeader("version", TestNetParams.testHeader.version);
        request.AddHeader("walletAddress", TestNetParams.testHeader.walletAddress);
        request.AddHeader("timezone", DataUtils.GetLocalTimeZone());

        if (Application.platform == RuntimePlatform.Android)
        {
            request.AddHeader("mobile", "android");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            request.AddHeader("mobile", "ios");
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            // 编辑器状态下默认视为android
            request.AddHeader("mobile", "android");
        }
        request.Send();
    }
    
    public static void UnityLocalTest_MakeAsyncRequest(string _path, int _requestType, string _paramStr, UnityAction<string> onReceive, UnityAction<string> onFail)
    {
        string url = UnityLocalTest_CombineUrl(_path,_requestType,_paramStr);
        LoggerUtils.Log("##MakeAsyncRequest path:"+url + "   param:"+_paramStr);
        HTTPAsyncHelper.GetHTTPAsync(url,_requestType,_paramStr,(HttpPack httpPack)=>{
            LoggerUtils.Log("##MakeAsyncRequest返回数据 StatusCode："+httpPack.StatusCode + "   ResponeData:"+httpPack.ResponeData);
            if(httpPack.StatusCode < 200)
            {
                LoggerUtils.Log("##MakeAsyncRequest 请求失败用bestHttp重试");
                UnityLocalTest_MakeTestHttpRequest(_path,_requestType,_paramStr,onReceive,onFail);
            }
            else if (httpPack.StatusCode != 200)
            {
                LoggerUtils.Log("MakeAsyncRequest error：" + httpPack.StatusCode);
                SavingData.HttpResponseRaw responseDataRaw = new SavingData.HttpResponseRaw();
                responseDataRaw.result = -1;
                responseDataRaw.rmsg = "MakeAsyncRequest error:"+httpPack.StatusCode;
                onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
            }
            else if(string.IsNullOrEmpty(httpPack.ResponeData)){
                SavingData.HttpResponseRaw responseDataRaw = new SavingData.HttpResponseRaw();
                responseDataRaw.result = -1;
                responseDataRaw.rmsg = "MakeAsyncRequest ResponeData is null:"+httpPack.StatusCode;
                onFail?.Invoke(JsonConvert.SerializeObject(responseDataRaw));
            }
            else
            {
                LoggerUtils.Log("MakeAsyncRequest rsp content：" + httpPack.ResponeData);
                SavingData.HttpResponseRaw responseDataRaw = JsonConvert.DeserializeObject<SavingData.HttpResponseRaw>(httpPack.ResponeData);
                if (responseDataRaw.result != 0)
                {
                    LoggerUtils.Log("MakeAsyncRequest server result:" + responseDataRaw.result + "  content:" + httpPack.ResponeData);
                    onFail?.Invoke(httpPack.ResponeData);
                }
                else
                {
                    SavingData.HttpResponse responseData = new SavingData.HttpResponse
                    {
                        identifier = "",
                        isSuccess = 0 //1 success
                    };
                    responseData.data = JsonConvert.SerializeObject(responseDataRaw.data);
                    responseData.isSuccess = 1;
                    string resultRspSuccess = JsonConvert.SerializeObject(responseData);
                    onReceive?.Invoke(resultRspSuccess);
                }
            }
        });
    }
    
    public static string UnityLocalTest_CombineUrl(string _path, int _requestType, string _paramStr)
    {
        string url = baseUrl;
        if (!string.IsNullOrEmpty(RequestUrl))
        {
            url = RequestUrl;
        }

#if UNITY_EDITOR
        url = TestNetParams.Inst.GetMakeHttpUrl();
#endif

        if (_requestType == (int)HTTP_METHOD.GET && !string.IsNullOrEmpty(_paramStr))
        {
            JObject jObject = JObject.Parse(_paramStr);
            IEnumerable<string> nameValues = jObject
                .Properties()
                .Select(x => $"{x.Name}={x.Value}");
            url += _path + "?" + string.Join("&", nameValues);
        }
        else
        {
            url += _path;
        }
        return url;
    }


#endif
    #endregion
}

