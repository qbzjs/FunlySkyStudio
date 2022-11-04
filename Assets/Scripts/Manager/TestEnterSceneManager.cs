using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SavingData;

public class TestEnterSceneManager : MonoBehaviour
{
    public static void InitSceneData()
    {
        GameManager.Inst.engineEntry = new EngineEntry()
        {
            sceneType = (int)SCENE_TYPE.Downtown,
            subType = (int)EnterGameMode.GuestScene,
        };

        GameManager.Inst.baseGameJsonData = new BaseGameJson();
        GameManager.Inst.unityConfigInfo = new UnityConfigInfo()
        {

        };
        GameManager.Inst.ugcUntiyMapDataInfo = new UgcUntiyMapDataInfo()
        {

        };
        GameManager.Inst.onLineDataInfo = new OnLineDataInfo()
        {

        };
        GameManager.Inst.isInWhiteList = 0;
        GameManager.Inst.ugcUserInfo = new UserInfo()
        {

        };
        GameManager.Inst.ugcClothInfo = new UGCClothInfo()
        {

        };
        GlobalFieldController.IsDowntownEnter = true;
    }
}
