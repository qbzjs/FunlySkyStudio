/// <summary>
/// Author:WeiXin
/// Description:wwise声音管理器
/// Date: 2022-02-17
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundCookie
{
    public string name;
    public GameObject obj;
    public uint max;
    public uint count;
    public uint playingID;

    public SoundCookie(string name = "", GameObject obj = null, uint max = 0, uint count = 0)
    {
        this.name = name;
        this.obj = obj;
        this.max = max;
        this.count = count;
    }

    public void Stop()
    {
        max = 0;
        AkSoundEngine.StopPlayingID(playingID);
    }
    
    public void StopLoop()
    {
        max = 0;
    }
}

public struct FootSoundInfo
{
    public string switchState;
    public float deltaTime; 
}

public class AKSoundManager : CInstance<AKSoundManager>
{
    private Dictionary<string, string> footSoundDict = new Dictionary<string, string>();
    private Dictionary<string, string[]> emoSoundDict = new Dictionary<string, string[]>();
    private Dictionary<GameObject, GameObject> nodeDict = new Dictionary<GameObject, GameObject>();
    private string group = String.Empty;
    private string state = String.Empty;
    private string eventName = String.Empty;
    private GameObject go;

    private const string nodeName = "soundNode";

    public bool isOpenFootSound = true;

    public AKSoundManager()
    {
        initData();
        isOpenFootSound = GlobalSettingManager.Inst.IsFootstepOpen();
    }
    
    public void flySound(GameObject obj, bool play = false)
    {
        if (isOpenFootSound && play)
        {
            PostEvent("play_fly", obj);
        }
        else
        {
            PostEvent("stop_fly", obj);
        }
    }

    public void steeringWheelSound(GameObject obj, bool play = false)
    {
        if (isOpenFootSound && play)
        {
            PostEvent("play_steeringwheel", obj);
        }
        else
        {
            PostEvent("stop_steeringwheel", obj);
        }
    }

    public SoundCookie playEmoSound(string emoName, GameObject player, bool isSelf, uint count = 1)
    {
        var p = isSelf ? "1p" : "3p";
        string group = string.Empty;
        string eventName = string.Empty;
        var state = emoName.ToLower();
        if (emoSoundDict.ContainsKey(state))
        {
            group = emoSoundDict[state][0];
            eventName = emoSoundDict[state][1] + p;
        }
        else
        {
            return null;
        }
        CheckSoundNode(player);
        Action act = () =>
        {
            SetSwitch(group, state, player);
        };
        SoundCookie sc = new SoundCookie(eventName, player, count);
        if (count <= 1) //单次
        {        
            act.Invoke();
            sc = OnceSound(sc);
        }
        else //循环
        {
            RepeatSound(sc, act);
        }

        return sc;
    }

    public SoundCookie OnceSound(SoundCookie cookie)
    {
        cookie.playingID = PostEvent(cookie.name, cookie.obj);
        return cookie;
    }

    public void RepeatSound(SoundCookie cookie, Action act = null)
    {
        if (cookie == null) return;
        if (cookie.obj == null || cookie.max <= cookie.count) return;
        act?.Invoke();
        cookie.playingID = AkSoundEngine.PostEvent(cookie.name, cookie.obj, (uint)AkCallbackType.AK_EndOfEvent,
            (sco, ty, info) =>
            {
                SoundCookie sc = sco as SoundCookie;
                if (sc != null)
                {
                    sc.count += 1;
                    RepeatSound(sc, act);
                }
            }, cookie);
    }

    private void CheckSoundNode(GameObject root)
    {
        if (root != null && !nodeDict.ContainsKey(root))
        {
            var obj = new GameObject(nodeName);
            obj.transform.SetParent(root.transform);
            nodeDict.Add(root, obj);
        }
    }
    
    public void PlayJumpSound(string in_pszEventName, GameObject playGO)
    {
        if (!isOpenFootSound) return;
        PostEvent(in_pszEventName, playGO);
    }

    public void PlayFootSound(string in_pszEventName, string in_pszSwitchGroup, GameObject hitGo,
        GameObject playGO)
    {
        var  info = GetPlayerFootSoundInfo();
        PlayFootSound(in_pszEventName, in_pszSwitchGroup, hitGo, playGO, info.switchState);
    }

    public FootSoundInfo GetPlayerFootSoundInfo()
    {
        FootSoundInfo info = new FootSoundInfo();
        info.switchState = StandOnAudioType.defaultAudio;
        info.deltaTime = 0.26f;
        if (PlayerSwimControl.Inst && PlayerSwimControl.Inst.isInWater)
        {
            info.switchState = StandOnAudioType.waterAudio;
            info.deltaTime = 0.8f;
        }
        else if (PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnIceCube())
        {
            info = IceCubeManager.Inst.IceCubeSimpleOrMult(info);
        }
        else if (PlayerStandonControl.Inst && PlayerStandonControl.Inst.IsStandOnSnowCube())
        {
            info = SnowCubeManager.Inst.GetSimpleOrMultSoundInfo(info);
        }
        else if(GlobalFieldController.curMapMode == MapMode.Downtown)
        {
            info = SnowCubeManager.Inst.GetSimpleOrMultSoundInfo(info);
        }
        return info;
    }
    
    public void OtherPlayGroundSound(string in_pszEventName, string in_pszSwitchGroup, GameObject hitGo,
        GameObject playGO, StandOnType type)
    {
        var info = GetOtherGroundSoundInfo(type);
        PlayFootSound(in_pszEventName, in_pszSwitchGroup, hitGo, playGO, info.switchState);
    }

    public FootSoundInfo GetOtherGroundSoundInfo(StandOnType type)
    {
        FootSoundInfo info = new FootSoundInfo();
        info.switchState = StandOnAudioType.defaultAudio;
        info.deltaTime = 0.26f;
        switch (type)
        {
            case StandOnType.IceCube:
                info = IceCubeManager.Inst.IceCubeSimpleOrMult(info);
                break;
            case StandOnType.SnowCube:
                info = SnowCubeManager.Inst.GetSimpleOrMultSoundInfo(info);
                break;
            default:
                break;
        }
        if(GlobalFieldController.curMapMode == MapMode.Downtown)
        {
            info = SnowCubeManager.Inst.GetSimpleOrMultSoundInfo(info);
        }
        return info;
    }

    public FootSoundInfo GetOtherFootSoundInfo(StandOnType type)
    {
        FootSoundInfo info = new FootSoundInfo();
        info.switchState = StandOnAudioType.defaultAudio;
        info.deltaTime = 0.26f;
        switch (type)
        {
            case StandOnType.Nothing:
                break;
            case StandOnType.IceCube:
                info = IceCubeManager.Inst.IceCubeSimpleOrMult(info);
                break;
            case StandOnType.Water:
                info.switchState = StandOnAudioType.waterAudio;
                info.deltaTime = 0.8f;
                break;
            case StandOnType.SnowCube:
                info = SnowCubeManager.Inst.GetSimpleOrMultSoundInfo(info);
                break;
            default:
                break;
        }
        return info;
    }

    public void PlayFootSound(string in_pszEventName, string in_pszSwitchGroup, GameObject hitGo,
        GameObject playGO,string audioState = "default")
    {
        if (!isOpenFootSound) return;
        string s = audioState == StandOnAudioType.defaultAudio ? GetFootSwitchEvent(hitGo, playGO) : audioState;
        PlaySFootSoundFromState(in_pszEventName, in_pszSwitchGroup, s, playGO);
    }

    private void PlaySFootSoundFromState(string in_pszEventName, string in_pszSwitchGroup, string in_pszSwitchState,
        GameObject in_gameObjectID)
    {
        if (in_pszSwitchGroup == group && in_pszSwitchState == state && in_gameObjectID == go)
        {
        }
        else
        {
            group = in_pszSwitchGroup;
            state = in_pszSwitchState;
            go = in_gameObjectID;
            SetSwitch(in_pszSwitchGroup, in_pszSwitchState, in_gameObjectID);
            CheckSoundNode(in_gameObjectID);
        }

        PostEvent(in_pszEventName, in_gameObjectID);
    }

    private string GetFootSwitchEvent(GameObject hitGO, GameObject playGO)
    {
        string soundType = "default";
        if (hitGO != null && hitGO.transform.parent != null)
        {
            bool hasMatComp = false;
            var bevCom = hitGO.transform.parent.GetComponentInParent<NodeBaseBehaviour>();
            if (bevCom != null)
            {
                hasMatComp = bevCom.entity.HasComponent<MaterialComponent>();
            }

            //基础材质
            if (hasMatComp == true)
            {
                MaterialComponent matCom = bevCom.entity.Get<MaterialComponent>();
                string soundKey = (matCom.matId + 10000).ToString();
                if (footSoundDict.ContainsKey(soundKey))
                {
                    soundType = footSoundDict[soundKey];
                }
            }
            else
            {
                Renderer renderer = hitGO.GetComponentInChildren<Renderer>();
                if (renderer)
                {
                    //材质
                    string matName = renderer.material.name;
                    if (footSoundDict.ContainsKey(matName))
                    {
                        soundType = footSoundDict[matName];
                    }
                }
                else if (hitGO.GetComponentInChildren<Terrain>())
                {
                    Terrain terrain = hitGO.GetComponentInChildren<Terrain>();
                    int terrainTexIndex = GameUtils.GetTrrainTextureIndex(playGO.transform.position);
                    TerrainLayer terrainLayer = terrain.terrainData.terrainLayers[terrainTexIndex];
                    if (terrainLayer)
                    {
                        if (footSoundDict.ContainsKey(terrainLayer.name))
                        {
                            soundType = footSoundDict[terrainLayer.name];
                        }
                    }
                }
            }
        }

        return soundType;
    }

    public void PlayLadderSound( string name,GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent(name, in_gameObjectID);
    }

    public void PlaySeesawSound(string name, GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent(name, in_gameObjectID);
    }
    
    public void PlaySwingSound(string name, GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent(name, in_gameObjectID);
    }
    
    public void PlaySwimSound(string in_pszEventName, bool isOnWater, GameObject in_gameObjectID)
    {
        string state = "";
        state = isOnWater ? "onwater" : "underwater";
        
        SetSwitch("swim", state, in_gameObjectID);
        PostEvent(in_pszEventName, in_gameObjectID);
    }

    public void PlayAttackSound(string switchName,string in_pszEventName, string in_pszSwitchGroup, GameObject in_gameObjectID)
    {
        SetSwitch(in_pszSwitchGroup, switchName, in_gameObjectID);
        PostEvent(in_pszEventName, in_gameObjectID);
    }

    public void PlayVoiceDemoSound(string switchName, GameObject in_gameObjectID)
    {
        SetSwitch("Voice_Changer", switchName, in_gameObjectID);
        PostEvent("Play_Voice_Changer", in_gameObjectID);
    }
    public void PlayBounceplankSound(string switchName, GameObject in_gameObjectID)
    {
        SetSwitch("trampoline_level", switchName, in_gameObjectID);
        PostEvent("play_trampoline_level", in_gameObjectID);
    }
    public void StopVoiceDemoSound( GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent("Stop_Voice_Changer", in_gameObjectID);
    }
    public void PlayCrystalStoneLoop(GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent("Play_GreatSnowfield_Ice_Gem_Shine_Loop", in_gameObjectID);
    }
    public void StopCrystalStoneLoop(GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent("Stop_GreatSnowfield_Ice_Gem_Shine_Loop", in_gameObjectID);
    }
    public void PlayCrystalStonePickUp(GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent("Play_GreatSnowfield_Ice_Gem_PickUp", in_gameObjectID);
    }
    public void PlayHitSound(string switchName,string in_pszEventName, string in_pszSwitchGroup, GameObject in_gameObjectID)
    {
        SetSwitch(in_pszSwitchGroup, switchName, in_gameObjectID);
        PostEvent(in_pszEventName, in_gameObjectID);
    }

    public void PlayDeathSound(GameObject in_gameObjectID)
    {
        SetSwitch("", "", in_gameObjectID);
        PostEvent("play_pvp_bloodless_disappear", in_gameObjectID);
    }

    public void PlayTrapHitSound(GameObject in_gameObjectID)
    {
        PostEvent("Play_TrapHit", in_gameObjectID);
    }

    public AKRESULT SetSwitch(string in_pszSwitchGroup, string in_pszSwitchState, GameObject in_gameObjectID)
    {
        return AkSoundEngine.SetSwitch(in_pszSwitchGroup, in_pszSwitchState, in_gameObjectID);
    }

    public uint PostEvent(string in_pszEventName, GameObject in_gameObjectID)
    {
        return AkSoundEngine.PostEvent(in_pszEventName, in_gameObjectID);
    }

    private void initData()
    {
        footSoundDict.Clear();
        nodeDict.Clear();
        //ground
        footSoundDict.Add("Ground_1 (Instance)", "glass"); //玻璃地板
        footSoundDict.Add("Ground_2 (Instance)", "wood"); //木地板
        footSoundDict.Add("Ground_3 (Instance)", "wood"); //木地板
        footSoundDict.Add("Ground_4 (Instance)", "grave"); //草地
        footSoundDict.Add("Ground_5 (Instance)", "grave"); //草地
        footSoundDict.Add("Ground_6 (Instance)", "grave"); //草地
        footSoundDict.Add("Ground_7 (Instance)", "tile"); //瓷砖
        footSoundDict.Add("Ground_14 (Instance)", "wood"); //木地板
        footSoundDict.Add("Ground_15 (Instance)", "tile"); //瓷砖
        footSoundDict.Add("Ground_16 (Instance)", "tile"); //瓷砖

        footSoundDict.Add("oasis_grass (Instance)", "grave"); //草地：模板2底板
        footSoundDict.Add("sealand_bottom (Instance)", "deepwater"); //水面：海岛模板水面

        footSoundDict.Add("Ground_20 (Instance)", "masorry");//石头
        footSoundDict.Add("Ground_21 (Instance)", "beach");//沙地
        footSoundDict.Add("Ground_22 (Instance)", "grave");//草地
        footSoundDict.Add("Ground_25 (Instance)", "carpet");//地毯
        footSoundDict.Add("Ground_26 (Instance)", "carpet");//地毯
        footSoundDict.Add("Ground_27 (Instance)", "carpet");//地毯
        footSoundDict.Add("Ground_28 (Instance)", "carpet");//地毯
        footSoundDict.Add("Ground_29 (Instance)", "carpet");//地毯
        footSoundDict.Add("Ground_30 (Instance)", "carpet");//地毯
        footSoundDict.Add("Ground_31 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_32 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_33 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_49 (Instance)", "bamboo_mat");
        footSoundDict.Add("Ground_54 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_55 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_56 (Instance)", "matel");//铁板
        footSoundDict.Add("Ground_57 (Instance)", "masonry");//草砖
        footSoundDict.Add("Ground_58 (Instance)", "grave");//树叶草地
        footSoundDict.Add("Ground_59 (Instance)", "grave");//树叶草地
        footSoundDict.Add("Ground_60 (Instance)", "matel");//铁皮
        footSoundDict.Add("Ground_61 (Instance)", "matel");//铁皮
        footSoundDict.Add("Ground_62 (Instance)", "tile");//环形砖
        footSoundDict.Add("Ground_63 (Instance)", "wood");//木板
        footSoundDict.Add("Ground_64 (Instance)", "dirt");//泥土
        footSoundDict.Add("Ground_67 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_68 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_72 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_73 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_74 (Instance)", "grave");//树叶草地
        footSoundDict.Add("Ground_75 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_76 (Instance)", "masonry");//草砖
        footSoundDict.Add("Ground_77 (Instance)", "matel");//铁皮
        footSoundDict.Add("Ground_78 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_80 (Instance)", "grave");//树叶草地
        footSoundDict.Add("Ground_81 (Instance)", "wood");//木地板
        footSoundDict.Add("Ground_82 (Instance)", "wood");//木地板
        footSoundDict.Add("Ground_83 (Instance)", "wood");//木地板
        footSoundDict.Add("Ground_106 (Instance)", "wood");//木地板
        footSoundDict.Add("Ground_107 (Instance)", "wood");//木地板
        footSoundDict.Add("Ground_110 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_112 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_113 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_114 (Instance)", "tile");//瓷砖
        footSoundDict.Add("Ground_118 (Instance)", "bamboo_mat");
        footSoundDict.Add("Ground_122 (Instance)", "bamboo_mat");
        footSoundDict.Add("Ground_123 (Instance)", "tile");
        footSoundDict.Add("Ground_124 (Instance)", "tile");
        footSoundDict.Add("Ground_125 (Instance)", "wood");
        footSoundDict.Add("Ground_126 (Instance)", "wood");
        footSoundDict.Add("Ground_129 (Instance)", "tile");
        footSoundDict.Add("Ground_130 (Instance)", "tile");
        footSoundDict.Add("Ground_131 (Instance)", "masonry");
        footSoundDict.Add("Ground_132 (Instance)", "tile");
        footSoundDict.Add("Ground_138 (Instance)", "tile");
        footSoundDict.Add("Ground_139 (Instance)", "tile");
        footSoundDict.Add("Ground_145 (Instance)", "grave");
        footSoundDict.Add("Ground_148 (Instance)", "masonry");
        footSoundDict.Add("Ground_149 (Instance)", "matel");
        footSoundDict.Add("Ground_150 (Instance)", "tile");
        footSoundDict.Add("Ground_151 (Instance)", "bamboo_mat");
        footSoundDict.Add("Ground_153 (Instance)", "tile");
        footSoundDict.Add("Ground_154 (Instance)", "tile");
        footSoundDict.Add("Ground_156 (Instance)", "snow");


        //基础素材
        footSoundDict.Add("10000", "default");
        footSoundDict.Add("10001", "glass");
        footSoundDict.Add("10002", "wood");
        footSoundDict.Add("10003", "wood");
        footSoundDict.Add("10004", "grave");
        footSoundDict.Add("10005", "grave");
        footSoundDict.Add("10006", "grave");
        footSoundDict.Add("10007", "default");
        footSoundDict.Add("10008", "default");
        footSoundDict.Add("10009", "tile");
        footSoundDict.Add("10010", "tile");
        footSoundDict.Add("10011", "tile");
        footSoundDict.Add("10012", "tile");
        footSoundDict.Add("10013", "masonry");
        footSoundDict.Add("10014", "masonry");
        footSoundDict.Add("10015", "matel");
        footSoundDict.Add("10016", "matel");
        footSoundDict.Add("10017", "wood");
        footSoundDict.Add("10018", "tile");
        footSoundDict.Add("10019", "masonry");
        footSoundDict.Add("10020", "default");
        footSoundDict.Add("10021", "tile");
        footSoundDict.Add("10022", "tile");
        footSoundDict.Add("10023", "masonry");
        footSoundDict.Add("10028", "carpet");
        footSoundDict.Add("10029", "carpet");
        footSoundDict.Add("10030", "carpet");
        footSoundDict.Add("10031", "carpet");
        footSoundDict.Add("10032", "carpet");
        footSoundDict.Add("10033", "tile");
        footSoundDict.Add("10034", "carpet");
        footSoundDict.Add("10035", "tile");
        footSoundDict.Add("10036", "tile");
        footSoundDict.Add("10037", "tile");
        footSoundDict.Add("10054", "bamboo_mat");
        footSoundDict.Add("10059", "tile");
        footSoundDict.Add("10060", "tile");
        footSoundDict.Add("10061", "matel");
        footSoundDict.Add("10062", "masonry");
        footSoundDict.Add("10063", "dirt");
        footSoundDict.Add("10064", "grave");
        footSoundDict.Add("10065", "grave");
        footSoundDict.Add("10066", "matel");
        footSoundDict.Add("10067", "matel");
        footSoundDict.Add("10068", "tile");
        footSoundDict.Add("10069", "wood");
        footSoundDict.Add("10072", "tile");
        footSoundDict.Add("10073", "tile");
        footSoundDict.Add("10077", "tile");
        footSoundDict.Add("10078", "tile");
        footSoundDict.Add("10079", "grave");
        footSoundDict.Add("10081", "tile");
        footSoundDict.Add("10083", "masonry");
        footSoundDict.Add("10084", "matel");
        footSoundDict.Add("10085", "tile");
        footSoundDict.Add("10088", "grave");
        footSoundDict.Add("10102", "wood");
        footSoundDict.Add("10103", "wood");
        footSoundDict.Add("10104", "wood");
        footSoundDict.Add("10106", "wood");
        footSoundDict.Add("10107", "wood");
        footSoundDict.Add("10110", "tile");
        footSoundDict.Add("10112", "tile");
        footSoundDict.Add("10113", "tile");
        footSoundDict.Add("10114", "tile");
        footSoundDict.Add("10118", "bamboo_mat");
        footSoundDict.Add("10122", "bamboo_mat");
        footSoundDict.Add("10123", "tile");
        footSoundDict.Add("10124", "tile");
        footSoundDict.Add("10125", "wood");
        footSoundDict.Add("10126", "wood");
        footSoundDict.Add("10129", "tile");
        footSoundDict.Add("10130", "tile");
        footSoundDict.Add("10131", "masonry");
        footSoundDict.Add("10132", "tile");
        footSoundDict.Add("10138", "tile");
        footSoundDict.Add("10139", "tile");
        footSoundDict.Add("10145", "grave");
        footSoundDict.Add("10148", "masonry");
        footSoundDict.Add("10149", "matel");
        footSoundDict.Add("10150", "tile");
        footSoundDict.Add("10151", "bamboo_mat");
        footSoundDict.Add("10153", "tile");
        footSoundDict.Add("10154", "tile");
        footSoundDict.Add("10156", "snow");



        //地形Layer
        footSoundDict.Add("green_01", "grave");
        footSoundDict.Add("green_02", "grave");
        footSoundDict.Add("green_03", "grave");
        footSoundDict.Add("green_04", "grave");
        footSoundDict.Add("beach_01", "beach"); //沙滩
        footSoundDict.Add("beach_02", "beach"); //沙滩
        footSoundDict.Add("beach_03", "beach"); //沙滩
        footSoundDict.Add("mountian_01", "beach"); //沙滩
        footSoundDict.Add("mountian_02", "beach"); //沙滩
        footSoundDict.Add("mountian_03", "beach"); //沙滩
        
        //表情name对应group、event
        emoSoundDict.Clear();
        emoSoundDict.Add("adore", new []{"expression","play_expression_"});
        emoSoundDict.Add("awkward", new []{"expression","play_expression_"});
        emoSoundDict.Add("brother", new []{"expression","play_expression_"});
        emoSoundDict.Add("bye", new []{"expression","play_expression_"});
        emoSoundDict.Add("cheer", new []{"expression","play_expression_"});
        emoSoundDict.Add("complacent", new []{"expression","play_expression_"});
        emoSoundDict.Add("cry", new []{"expression","play_expression_"});
        emoSoundDict.Add("dismay", new []{"expression","play_expression_"});
        emoSoundDict.Add("doubt", new []{"expression","play_expression_"});
        emoSoundDict.Add("flashes", new []{"expression","play_expression_"});
        emoSoundDict.Add("folie", new []{"expression","play_expression_"});
        emoSoundDict.Add("handclap", new []{"expression","play_expression_"});
        emoSoundDict.Add("hug", new []{"expression","play_expression_"});
        emoSoundDict.Add("kiss", new []{"expression","play_expression_"});
        emoSoundDict.Add("laugh", new []{"expression","play_expression_"});
        emoSoundDict.Add("mess", new []{"expression","play_expression_"});
        emoSoundDict.Add("no", new []{"expression","play_expression_"});
        emoSoundDict.Add("petrification", new []{"expression","play_expression_"});
        emoSoundDict.Add("pudency", new []{"expression","play_expression_"});
        emoSoundDict.Add("rage", new []{"expression","play_expression_"});
        emoSoundDict.Add("refuse", new []{"expression","play_expression_"});
        emoSoundDict.Add("rejoice", new []{"expression","play_expression_"});
        emoSoundDict.Add("sad", new []{"expression","play_expression_"});
        emoSoundDict.Add("shock", new []{"expression","play_expression_"});
        emoSoundDict.Add("sneer", new []{"expression","play_expression_"});
        emoSoundDict.Add("speak", new []{"expression","play_expression_"});
        emoSoundDict.Add("standhand", new []{"expression","play_expression_"});
        emoSoundDict.Add("stomp", new []{"expression","play_expression_"});
        emoSoundDict.Add("surrender", new []{"expression","play_expression_"});
        emoSoundDict.Add("tremble", new []{"expression","play_expression_"});
        emoSoundDict.Add("wave", new []{"expression","play_expression_"});
        emoSoundDict.Add("yell", new []{"expression","play_expression_"});
        emoSoundDict.Add("yes", new []{"expression","play_expression_"});

        emoSoundDict.Add("bow", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("bubble", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("coffee", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("coin", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dance01", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dance02", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dance03", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dance04", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dance05", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dance06", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dance07", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("dice", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("djembe_centre", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("djembe_end", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("djembe_start", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("encourage", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("fingerguessing", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("flower", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("hulahoop", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("nap", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("playup", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("salute", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("salvo", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("selfie", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("shuaishouwu", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("signno", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("signyes", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("sleep", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("sneeze", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("snowball", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("stretchoneself", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("sweep", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("thomas", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("trumpet", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("type", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("ukulele", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("whistle", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("yawn", new []{"single_action","play_single_action_"});
        emoSoundDict.Add("onemorebear", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("cat", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("sing_centre", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("vomit", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("barbell", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("pushup", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("umbrella_start", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("umbrella_end", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("gangnan_centre", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("electricguitar_start", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("electricguitar_centre", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("werewolf", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("harp_centre", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("streetball", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("beckdance_centre", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("fishknight_start", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("fishknight_centre", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("fishknight_end", new[] { "single_action", "play_single_action_" });

        emoSoundDict.Add("rehandling", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("bracelet20", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("bracelet21", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("stick", new[] { "single_action", "play_single_action_" });
        emoSoundDict.Add("broadsword03", new[] { "single_action", "play_single_action_" });

        emoSoundDict.Add("clap_finish", new []{"double_action","play_double_action_"});
        emoSoundDict.Add("shoulder_finish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("foreheadfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("fancygreetingfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("princesshugfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("footballfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("baseballfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("tapdance_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("tapdancefinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("fightfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("tomarryhimfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("takingpicturesfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("doublebowfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("magicfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("toastfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("win_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("winfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("tshowfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("lovefinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("labisefinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("feedfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("coconutfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("danceafinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("dancebfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("handshake_finish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("balloonfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("ventfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("heavyfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("heavy_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("fistfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("slapfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("bombfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("leapfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("bouncefinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("bullfight_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("bullfightfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("circlingfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("jumprope_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("jumpropefinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("death_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("deathfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("pumpkin_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("pumpkinfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("ghost_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("ghostfinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("whackamole_start", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("whackamolefinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("flyingknifefinish", new[] { "double_action", "play_double_action_" });
        emoSoundDict.Add("dodgefinish", new[] { "double_action", "play_double_action_" });


    }
}
