using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public Button LocalSaveMapJson;
    public Button LoadReadMapJson;

    public UnityBaseInfo tokenInfo = new UnityBaseInfo();
    public MapInfo mapInfo = new MapInfo();

    public GetGameJson getGameJson = new GetGameJson();


    private void Awake()
    {
        HttpUtils.tokenInfo = tokenInfo;
        //GameManager.Inst.gameMapInfo
    }

    
}
