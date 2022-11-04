using BudEngine.NetEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author : Tee Li
/// 描述：手电灯行为
/// 日期：2022/10/08
/// </summary>

public class FlashLightBehaviour : NodeBaseBehaviour
{
    private readonly string dirAnchorPath = "DirectionalModel/Dummy001/Bone001";
    private readonly string dirUpAnchorPath = "DirectionalModel/Dummy001/Bone002";
    private readonly string spotAnchorPath = "SpotLightModel/Dummy001/Bone001";
    private readonly string spotUpAnchorPath = "SpotLightModel/Dummy001/Bone002";

    private Light realLight;
    private Transform dirAnchor;
    private Transform dirAnchorUp;
    private Transform spotAnchor;
    private Transform spotAnchorUp;

    private Renderer bulb;
    private GameObject dirLightModel;
    private GameObject spotLightModel;
    private GameObject highGo;

    private SkinnedMeshRenderer dirDisc;
    private SkinnedMeshRenderer spotDisc;
    private SkinnedMeshRenderer dirBody;
    private SkinnedMeshRenderer spotBody;

    private List<Color> playingColorList;
    private int playingIndex;
    private bool isPlaying;
    private DeltaTimer timer;

    public Color editModeColor;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();

        realLight = GetComponentInChildren<Light>();

        dirAnchor = transform.Find(dirAnchorPath);
        dirAnchorUp = transform.Find(dirUpAnchorPath);
        spotAnchor = transform.Find(spotAnchorPath);
        spotAnchorUp = transform.Find(spotUpAnchorPath);

        bulb = transform.Find("BulbModel/Light").GetComponent<Renderer>();
        dirLightModel = transform.Find("DirectionalModel").gameObject;
        spotLightModel = transform.Find("SpotLightModel").gameObject;
        highGo = transform.Find("BulbModel/Shell").gameObject;

        dirDisc = dirLightModel.transform.Find("lightdi").GetComponent<SkinnedMeshRenderer>();
        dirBody = dirLightModel.transform.Find("lightzhuan").GetComponent<SkinnedMeshRenderer>();
        spotDisc = spotLightModel.transform.Find("lightdi").GetComponent<SkinnedMeshRenderer>();
        spotBody = spotLightModel.transform.Find("lightzhuan").GetComponent<SkinnedMeshRenderer>();


        timer = new DeltaTimer();
        playingColorList = new List<Color>();
    }

    public void SetUp()
    {
        FlashLightComponent comp = entity.Get<FlashLightComponent>();
        SetType(comp.type);
        SetIntensity(comp.inten);       
        SetRadius(comp.radius);
        SetRange(comp.range);
        SetIsReal(comp.isReal > 0);
        editModeColor = comp.colors.Count > 0 ? comp.colors[comp.colors.Count-1] : Color.white;
        SetColor(editModeColor);
    }

    public void OnChangeMode(GameMode mode)
    {
        if(mode == GameMode.Edit)
        {
            bulb.enabled = true;
            SetColor(editModeColor);
            EnterColorPlayMode(false);
        }

        if(mode == GameMode.Play || mode == GameMode.Guest)
        {
            bulb.enabled = false;
            EnterColorPlayMode(true);
        }
    }

    #region Setters
    public void SetType(int type)
    {
        switch ((FlashLightType)type)
        {
            case FlashLightType.Directional:
                dirLightModel.SetActive(true);
                spotLightModel.SetActive(false);
                break;
            case FlashLightType.SpotLight:
                dirLightModel.SetActive(false);
                spotLightModel.SetActive(true);
                break;
        }
    }

    public void SetIntensity(float value)
    {       
        SetIntensityToRender(value);
        realLight.intensity = DataUtils.GetRealValue(value, 0f, 8f, 0f, 1f);
    }


    public void SetRange(float value)
    {
        Vector3 pos = new Vector3(0f, 0f, -value);
        dirAnchor.localPosition = pos;
        spotAnchor.localPosition = pos;

        realLight.range = value;
        SyncSpotLightAngle();
        SyncBounds();
    }

    public void SetRadius(float value)
    {
        Vector3 scale = new Vector3(1f, value, value);
        dirAnchor.localScale = scale;
        dirAnchorUp.localScale = scale;
        spotAnchor.localScale = scale;
        spotAnchorUp.localScale = scale;

        SyncSpotLightAngle();
        SyncBounds();
    }


    public void SetIsReal(bool isReal)
    {
        realLight.enabled = isReal;
    }


    public void SetColor(Color color)
    {
        SetColorToRender(dirDisc, color);
        SetColorToRender(dirBody, color);       
        SetColorToRender(spotDisc, color);
        SetColorToRender(spotBody, color);
        realLight.color = color;
    }

    #endregion

    #region ColorPlayMode
    public void OnFixUpdate()
    {
        if (isPlaying)
        {
            timer.Elapse(Time.fixedDeltaTime);
            if (timer.Ring())
            {
                playingIndex += 1;
                if(playingIndex >= playingColorList.Count)
                {
                    playingIndex = 0;
                    playingColorList = GetNextList(entity.Get<FlashLightComponent>());
                }
                PlayCurrentColor();
                
                timer.Reset();
            }
        }
    }

    public void EnterColorPlayMode(bool isOn)
    {
        isPlaying = isOn;
        if (isOn)
        {
            FlashLightComponent comp = entity.Get<FlashLightComponent>();
            timer.Reset();
            timer.ResetTimeToRing(comp.time);

            GetStartIndexAndTime(comp, out playingIndex, out float timeElapsed);
            timer.Elapse(timeElapsed);
            playingColorList = GetNextList(comp);

            PlayCurrentColor();
        }        
    }


    #endregion

    #region Helpers

    //组合缩放时限制手电灯绝对大小不变
    public void FixScale()
    {
        if (transform.parent == null) return;
        Vector3 pScale = transform.parent.localScale;
        if (pScale.x <= 0 || pScale.y <= 0 || pScale.z <= 0) return;

        Vector3 myScale = new Vector3(1f / pScale.x, 1f / pScale.y, 1f / pScale.z);
        myScale = DataUtils.LimitVector3(myScale); //限制最小值为0.0001
        transform.localScale = myScale;
    }

    private void SetIntensityToRender(float inten)
    {
        float alpha = Mathf.Clamp01(inten);
        Color color = dirBody.material.GetColor("_Color");
        color.a = alpha;
        dirDisc.material.SetColor("_Color", color);
        dirBody.material.SetColor("_Color", color);
        spotDisc.material.SetColor("_Color", color);
        spotBody.material.SetColor("_Color", color);
        dirBody.material.SetTextureOffset("_MainTex", new Vector2(0, DataUtils.GetRealValue(alpha, 0, 0.5f, 0.1f, 0.8f)));
        spotBody.material.SetTextureOffset("_MainTex", new Vector2(0, DataUtils.GetRealValue(alpha, 0, 0.5f, 0.1f, 0.8f)));
    }

    private void SetColorToRender(Renderer renderer, Color color)
    {
        Color curColor = renderer.material.GetColor("_Color");
        Color newColor = new Color(color.r, color.g, color.b, curColor.a);
        renderer.material.SetColor("_Color", newColor);
    }

    private void SyncSpotLightAngle()
    {
        float radius = dirAnchor.localScale.z;
        float range = Mathf.Abs(dirAnchor.localPosition.z);
        realLight.spotAngle = Mathf.Rad2Deg * Mathf.Atan2(0.17f * radius, range) * 2f;
    }

    //骨骼位置改变时同步包围盒
    private void SyncBounds()
    {
        float range = Mathf.Abs(dirAnchor.transform.localPosition.z);
        float diameter = 0.17f * 2;
        
        Vector3 bodyCenter = new Vector3(dirAnchor.transform.localPosition.z / 2, 0 , 0);
        Vector3 bodySize = new Vector3(range, diameter, diameter);
        Bounds bodyBounds = new Bounds(bodyCenter, bodySize);
        
        Vector3 discCenter = Vector3.zero;
        Vector3 discSize = new Vector3(0.5f, diameter, diameter);
        Bounds discBounds = new Bounds(discCenter, discSize);

        dirBody.localBounds = bodyBounds;
        dirDisc.localBounds = discBounds;
        spotBody.localBounds = bodyBounds;
        spotDisc.localBounds = discBounds;
    }

    private void PlayCurrentColor()
    {
        if(playingIndex >= 0 && playingIndex < playingColorList.Count)
        {
            SetColor(playingColorList[playingIndex]);
        }
    }

    private List<Color> GetNextList(FlashLightComponent comp)
    {
        return (FlashLightMode)comp.mode switch
        {
            FlashLightMode.Queue => new List<Color>(comp.colors),
            FlashLightMode.Random => GetRandomizedList(comp.colors),
            _ => new List<Color>()
        };
    }

    private List<Color> GetRandomizedList(List<Color> color)
    {
        List<Color> copyList = new List<Color>(color);
        DataUtils.RandomShuffle(copyList);
        return copyList;
    }

    //返还 ：当前播放到哪个颜色 & 当前颜色播放了多少时间
    private void GetStartIndexAndTime(FlashLightComponent comp, out int startIndex, out float timeElapsed)
    {
        if (GlobalFieldController.CurGameMode == GameMode.Guest)
        {
            if (Global.Room == null || Global.Room.RoomInfo == null)
            {
                LoggerUtils.LogError("Enter Flashlight, Room may be null!");
                startIndex = 0;
                timeElapsed = 0f;
                return;
            }

            double curUnixTime = GameUtils.GetUtcTimeStampAsSpan().TotalSeconds; //精确到毫秒
            ulong roomCreateTime = Global.Room.RoomInfo.CreateTime;
            float timePassed = (float)(curUnixTime - roomCreateTime);  
            //如果用户时间错误，出现异常值，则从0开始
            if (timePassed < 0f)
            {
                LoggerUtils.LogError("User unix time error:" + curUnixTime);
                startIndex = 0;
                timeElapsed = 0f;
                return;
            }

            //若用户保存数据错误则返回默认值
            if(comp.time <= 0 || comp.colors.Count <= 0)
            {
                LoggerUtils.LogError("User flashlight data error: " + comp.GetAttr().v);
                startIndex = 0;
                timeElapsed = 0f;
                return;
            }

            int nIntervalPlayed = Mathf.FloorToInt(timePassed / comp.time);

            startIndex = nIntervalPlayed % comp.colors.Count;
            timeElapsed = timePassed - (nIntervalPlayed * comp.time);
        }
        else
        {
            startIndex = 0;
            timeElapsed = 0f;
        }
    }

    #endregion

    public override void HighLight(bool isHigh)
    {
        base.HighLight(isHigh);
        highGo.SetActive(isHigh);
    }
}



