using System;
using UnityEngine;

public class HttpDownloader : BaseAction
{
    private string url;
    private string fileName;
    private BestHTTP.HTTPRequest request;

    public Action<string> onStarted;
 

    public HttpDownloader(string url, Action<BaseAction, object, string> callBack, int priority = 0) : base(callBack, priority)
    {
        Debug.Log("HttpDownloader: " + url);

        this.url = url;
        this.fileName = GetFileName(url);
    }

    public string Url
    {
        get { return url; }
        set { url = value; }
    }

    public string FileName
    {
        get { return fileName; }
        set { fileName = value; }
    }

    public override void Do()
    {
        if (!isUsed)
        {
            return;
        }
        request = new BestHTTP.HTTPRequest(new Uri(url), BestHTTP.HTTPMethods.Get, true, true,
            OnRequestFinishedDelegate);
        request.MaxRetries = 2;
        request.Timeout = TimeSpan.FromSeconds(HttpUtils.HttpTimeout);
        request.Send();
        onStarted?.Invoke(fileName);
    }

    private string GetFileName(string url)
    {
        return url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
    }

    private void OnRequestFinishedDelegate(BestHTTP.HTTPRequest originalRequest, BestHTTP.HTTPResponse response)
    {
        if (!isUsed)
        {
            return;
        }
        if (originalRequest.State == BestHTTP.HTTPRequestStates.Finished && response.IsSuccess)
        {
            OnAssetCallBack(response.Data);
            request?.Dispose();
            request = null;
        }
        else
        {
            OnAssetCallBack(null, GetRequestError(originalRequest, response));
            request?.Dispose();
            request = null;
        }
    }

    private string GetRequestError(BestHTTP.HTTPRequest req, BestHTTP.HTTPResponse rsp)
    {
        var err = "request Error State:" + req.State;
        switch (req.State)
        {
            case BestHTTP.HTTPRequestStates.Finished:
                err = string.Format(
                    "Request finished Successfully, but the server sent an error. Status Code: {0}-{1} Message: {2}",
                    rsp.StatusCode,
                    rsp.Message,
                    rsp.DataAsText);
                break;
            case BestHTTP.HTTPRequestStates.Error:
                err = "Request Finished with Error! " + (req.Exception != null
                    ? (req.Exception.Message + "\n" + req.Exception.StackTrace)
                    : "No Exception");
                break;

            // The request aborted, initiated by the user.
            case BestHTTP.HTTPRequestStates.Aborted:
                err = "Request Aborted!";
                break;

            // Connecting to the server is timed out.
            case BestHTTP.HTTPRequestStates.ConnectionTimedOut:
                err = "Connection Timed Out!";
                break;

            // The request didn't finished in the given time.
            case BestHTTP.HTTPRequestStates.TimedOut:
                err = "Processing the request Timed Out!";
                break;
        }

        return err;
    }

    public override void Dispose()
    {
        base.Dispose();
        request?.Abort();
        request?.Dispose();
        request = null;
    }
}

