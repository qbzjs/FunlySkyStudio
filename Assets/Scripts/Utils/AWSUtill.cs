/// <summary>
/// Author:Mingo-LiZongMing
/// Description:AWS上传工具
/// </summary>
using UnityEngine;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.IO;
using Amazon.CognitoIdentity;
using Amazon;
using UnityEngine.Events;
using System;
using Newtonsoft.Json;

[Serializable]
public struct AWSRequest
{
    public string filePath;
    public string key;
    public string cannedAclHeader;
    public int timeout;
    public int businessType; //上传业务类型  1打卡相册
    public string businessData; //上传业务参数
}

[Serializable]
public struct AWSResponse
{
    public string filePath;
    public string key;
    public string message;
}

public enum LogEventType
{
    None,
    Png,
    Json,
}

public enum BusinessType
{
    Default,
    Photo, //打卡相册
}
public enum PhotoType
{
    QuickMode = 0,
    CameraMode = 1,
    SelfieMode = 2,
}

[Serializable]
public struct PhotoBusData
{
    public string ugcId;
    public string downtownId;
    public string downtownName;
    public string downtownCover;
    public string downtownDesc;
}

//打卡相册数据
[Serializable]
public struct AlbumRequest
{
    public string filePath;
    public string key;
    public string businessData; //上传业务参数
}

public class AWSUtill
{
    #region private members
    public static string IdentityPoolId = "us-west-1:1d4b51d3-3f5b-41a6-a335-18223545efce";
    public static string CognitoIdentityRegion = "us-west-1";
    public static RegionEndpoint _CognitoIdentityRegion
    {
        get { return RegionEndpoint.GetBySystemName(CognitoIdentityRegion); }
    }
    public static string S3Region = "us-west-1";
    public static RegionEndpoint _S3Region
    {
        get { return RegionEndpoint.GetBySystemName(S3Region); }
    }
    public static string S3BucketName = "buddy-app-bucket";

    public static IAmazonS3 _s3Client;
    public static AWSCredentials _credentials;
    public static string SavePath = "";
    public static string urlRoot = "https://cdn.joinbudapp.com/";
    public static string PropsJsonPath = "TestFolder/" + "PropsJson/";
    public static string JsonPath = "TestFolder/" + "UgcJson/";
    public static string ImagePath =  "TestFolder/" + "UgcImage/";
    public static string videoPath =  "TestFolder/" + "UgcBgVedioSource/";

    public static string propsZipFilePath = "TestFolder/" + "PropsZipFile/";
    public static string zipFilePath = "TestFolder/" + "UgcZipFile/";

    public static string ugcClothJsonPath = "TestFolder/" + "U3D/UGCClothes/ClothJson/";
    public static string ugcClothTexPath = "TestFolder/" + "U3D/UGCClothes/ClothTex/";
    

    public static AWSCredentials Credentials
    {
        get
        {
            if (_credentials == null)
                _credentials = new CognitoAWSCredentials(IdentityPoolId, _CognitoIdentityRegion);
            return _credentials;
        }
    }

    private static IAmazonS3 Client
    {
        get
        {
            if (_s3Client == null)
            {
                _s3Client = new AmazonS3Client(Credentials, new AmazonS3Config
                {
                    RegionEndpoint = _S3Region,
                    UseAccelerateEndpoint = true
                });
            }
            //test comment
            return _s3Client;
        }
    }

    #endregion

    public static void SetAwsSavingPath()
    {
        PropsJsonPath = "PropsJson/" + GameInfo.Inst.myUid + "/";
        JsonPath = "UgcJson/" + GameInfo.Inst.myUid + "/";
        ImagePath = "UgcImage/" + GameInfo.Inst.myUid + "/";
        videoPath = "UgcBgVedioSource/" + GameInfo.Inst.myUid + "/";
        propsZipFilePath = "PropsZipFile/" + GameInfo.Inst.myUid + "/";
        zipFilePath = "UgcZipFile/" + GameInfo.Inst.myUid + "/";
        ugcClothJsonPath = "U3D/UGCClothes/ClothJson/" + GameInfo.Inst.myUid + "/";
        ugcClothTexPath = "U3D/UGCClothes/ClothTex/" + GameInfo.Inst.myUid + "/";
    }

    public static string GetFileUrl(string filePath)
    {
        return urlRoot + filePath;
    }


    //检查文件是否存在
    private static bool CheckFile(string fileName,string filePath)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            LoggerUtils.LogError("AWSUtitll.CheckFile -- fileName is Empty");
            return false;
        }

        if (!File.Exists(filePath))
        {
            LoggerUtils.Log("AWSUtitll.CheckFile -- File not Exist");
            return false;
        }

        return true;
    }

    //组装Unity上传AWS所需参数
    public static PostObjectRequest GetRequestStruct(string filePath,string fileName)
    {
        if (!File.Exists(filePath))
        {
            LoggerUtils.LogError("GetRequestStruct !File.Exists()" + filePath);
        }
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var requestStruct = new PostObjectRequest()
        {
            Bucket = S3BucketName,
            Key = fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.Private,
            Region = _S3Region
        };
        return requestStruct;
    }


    //组装Navite所需参数
    public static AWSRequest GetMobileReqStruct(string filePath, string fileName, int timeout = -1, int businessType = 0, string businessData = "")
    {
        if (!File.Exists(filePath))
        {
            LoggerUtils.LogError("GetMobileReqStruct !File.Exists()" + filePath);
        }
        var awsRequest = new AWSRequest()
        {
            filePath = filePath,
            key = fileName,
            cannedAclHeader = "private",
            businessType = businessType,
            businessData = businessData,
            timeout = timeout,

        };
        return awsRequest;
    }

    //调用Native接口上传
    private static void PostObjectByMobile(AWSRequest awsRequest, LogEventType type, Action<string> onSuccess, Action<string> onFail, bool isRole = false, bool isDelete = true)
    {
        if (type != LogEventType.None) MobileInterface.Instance.LogEventByEventName("unity_upload" + type.ToString() + "_start");
        MobileInterface.Instance.AddClientRespose(awsRequest.filePath, (resp) =>
        {
            AWSResponse awsResp = JsonConvert.DeserializeObject<AWSResponse>(resp);
            if (type != LogEventType.None) MobileInterface.Instance.LogEventByEventName("unity_upload" + type.ToString() + "_success");
            MobileInterface.Instance.DelClientResponse(awsResp.filePath);
            string url = GetFileUrl(awsResp.key);
            LoggerUtils.Log("AwsUploadSuccess");
            onSuccess?.Invoke(url);
            if (isDelete) File.Delete(awsResp.filePath);
        });
        MobileInterface.Instance.AddClientFail(awsRequest.filePath, (resp) =>
        {
            AWSResponse awsResp = JsonConvert.DeserializeObject<AWSResponse>(resp);
            LoggerUtils.Log("AwsUploadFail");
            onFail?.Invoke(awsResp.message);
        });
        MobileInterface.Instance.UploadToAws(JsonConvert.SerializeObject(awsRequest), isRole);
    }

    //调用Unity接口上传AWS
    private static void PostObjectByAWS(PostObjectRequest postRequest,string filePath, Action<string> onSuccess, Action<string> onFail, bool isRole = false, bool isDelete = true)
    {
        Client.PostObjectAsync(postRequest, (responseObj) =>
        {
            if (responseObj.Exception == null)
            {
                string photoUrl = GetFileUrl(postRequest.Key);
                LoggerUtils.Log("UploadPhoto Success --> " + photoUrl);
                onSuccess?.Invoke(photoUrl);
                if (isDelete) File.Delete(filePath);
            }
            else
            {
                string msg = responseObj.Exception.Message;
                LoggerUtils.Log("UploadPhoto Fail --> " + msg);
                onFail?.Invoke(msg);
            }
        });
    }

    //上传照片默认90s超时
    public static void UpLoadPhoto(string fileName, string photoData, Action<string> onSuccess, Action<string> onFail, int timeout = 90)
    {
        var filePath = DataUtils.dataDir + fileName;
        if(!CheckFile(fileName,filePath))
        {
            onFail?.Invoke(null);
            return;
        }
        var awsName = ImagePath + fileName;
        LoggerUtils.Log("UpLoadPhoto awsName = " + awsName);
#if UNITY_EDITOR
        var requestStruct = GetRequestStruct(filePath, awsName);
        PostObjectByAWS(requestStruct,filePath,onSuccess,onFail);
#else
        var req = GetMobileReqStruct(filePath, awsName,timeout, (int)BusinessType.Photo, photoData);
        PostObjectByMobile(req, LogEventType.Png, null, null);
#endif
    }
    public static void UpLoadToAlbum(string fileName, string photoData, Action<string> onSuccess, Action<string> onFail, int photoType)
    {
        var filePath = DataUtils.dataDir + fileName;
        if(!CheckFile(fileName,filePath))
        {
            onFail?.Invoke(null);
            return;
        }
        var awsName = ImagePath + fileName;
        LoggerUtils.Log("UpLoadToAlbum awsName = " + awsName);
#if UNITY_EDITOR
        var requestStruct = GetRequestStruct(filePath, awsName);
        PostObjectByAWS(requestStruct,filePath,onSuccess,onFail);
#else
        onSuccess?.Invoke("Added photo to album!");
        AlbumRequest request = new AlbumRequest()
        {
            filePath = filePath,
            key = awsName,
            businessData = photoData,
        };
        MobileInterface.Instance.UploadToAlbum(JsonConvert.SerializeObject(request));
        DataLogUtils.LogUploadAlbumStart(fileName,photoType);
#endif
    }

    //上传图片如：上传素材封面截图、玩家人物形象截图、服装搭配
    public static void UpLoadImage(string fileName, Action<string> onSuccess, Action<string> onFail, bool isRole = false, int timeout = -1)
    {
        var filePath = DataUtils.dataDir + fileName;
        if(!CheckFile(fileName,filePath))
        {
            onFail?.Invoke(null);
            return;
        }
        var awsName = ImagePath + fileName;
        LoggerUtils.Log("UpLoadImage awsName = " + awsName);
#if UNITY_EDITOR
        var requestStruct = GetRequestStruct(filePath, awsName);
        PostObjectByAWS(requestStruct,filePath,onSuccess,onFail);
#else
        var req = GetMobileReqStruct(filePath, awsName,timeout);
        PostObjectByMobile(req, LogEventType.Png, onSuccess, onFail, isRole);
#endif
    }

    //上传资源如：本地定位日志
    public static void UpLoadRes(string fileName, string fullPath,string awsPath, Action<string> onSuccess, Action<string> onFail,bool isDel = false)
    {
        if(!CheckFile(fileName,fullPath))
        {
            onFail?.Invoke(null);
            return;
        }
        var awsName = awsPath + fileName;
        LoggerUtils.Log("UpLoadRes awsName = " + awsName);
#if UNITY_EDITOR
        var requestStruct = GetRequestStruct(fullPath, awsName);
        PostObjectByAWS(requestStruct,fullPath,onSuccess,onFail,false,isDel);
#else
        var req = GetMobileReqStruct(fullPath, awsName);
        PostObjectByMobile(req, LogEventType.None, onSuccess, onFail, false, isDel);
#endif
    }

    
    //上传素材json压缩包
    public static void UpLoadPropZipRes(string fileName, Action<string> onSuccess, Action<string> onFail)
    {
        var filePath = ZipUtils.zipDir + fileName;
        if(!CheckFile(fileName,filePath))
        {
            onFail?.Invoke(null);
            return;
        }
        
        var awsName = propsZipFilePath + fileName;
#if UNITY_EDITOR
        var requestStruct = GetRequestStruct(filePath, awsName);
        PostObjectByAWS(requestStruct,filePath,onSuccess,onFail);
#else
        var req = GetMobileReqStruct(filePath, awsName);
        PostObjectByMobile(req, LogEventType.None, onSuccess, onFail);
#endif
    }
}
