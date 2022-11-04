using System.Security.Cryptography.X509Certificates;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net.Security;

public class HttpPack
{
    public int StatusCode;
    public string ResponeData;

}
public class HttpThreadParam
{
    public string url;
    public int requestType;
    public string paramStr;
    public Action<HttpPack> onFinish;
}

public static class HTTPAsyncHelper
{
    private const int HttpTimeout = 10;
    private static Encoding mEncode = Encoding.UTF8;

    public static void Init()
    {
        LoggerUtils.Log("##HttpAsyncHelper Init"); 
        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Ssl3;
    }
    public static void GetHTTPAsync(string _url, int _requestType, string _paramStr, Action<HttpPack> callback)
    {

        HttpThreadParam threadParam = new HttpThreadParam()
        {
            url = _url,
            requestType = _requestType,
            paramStr = _paramStr,
            onFinish = callback
        };
        //开启子线程
        ThreadPool.QueueUserWorkItem(AsyncHttpThreadFunc, threadParam);
    }

    private static void AsyncHttpThreadFunc(object param)
    {
        HttpThreadParam threadParam = param as HttpThreadParam;
        string url = threadParam.url;
        var callback = threadParam.onFinish;
        string paramStr = threadParam.paramStr;

        HttpWebRequest webRequest = null;
        webRequest = WebRequest.Create(url) as HttpWebRequest;
        webRequest.ProtocolVersion = HttpVersion.Version10;
        HttpUtils.InitHttpHeader(webRequest);
        webRequest.Accept = "application/json";
        webRequest.Timeout = HttpTimeout * 1000;
        webRequest.UseDefaultCredentials = false;

        // LoggerUtils.Log("#####webRequest.Proxy:" + webRequest.Proxy.ToString());
        // webRequest.Proxy = null;

        if (threadParam.requestType == (int)HTTP_METHOD.POST)
        {
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            if (!string.IsNullOrEmpty(paramStr))
            {
                byte[] byteArray = mEncode.GetBytes(paramStr);
                using (Stream stream = webRequest.GetRequestStream())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                }
            }
        }
        else
        {
            webRequest.Method = "GET";
            webRequest.ContentType = "application/json";
        }

        string responseContent = "";
        HttpPack pack = new HttpPack();
        try
        {
            using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
            {
                pack.StatusCode = (int)response.StatusCode;
                using (Stream resStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(resStream, mEncode))
                    {
                        responseContent = reader.ReadToEnd().ToString();
                    }
                }
            }
        }
        catch (WebException ex)
        {
            UnityEngine.Debug.Log("##AsyncHttpThreadFunc exception：" + ex.ToString());
            pack.StatusCode = (int)ex.Status;
        }

        UnityEngine.Debug.Log("##AsyncHttpThreadFunc respone: statusCode =" + pack.StatusCode + "   content:" + responseContent);
        pack.ResponeData = responseContent;
        callback.Invoke(pack);
        webRequest.Abort();
    }

    //SSL认证总是接受 
    private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        LoggerUtils.Log("##认证 ssl");
        return true; 
    }

}