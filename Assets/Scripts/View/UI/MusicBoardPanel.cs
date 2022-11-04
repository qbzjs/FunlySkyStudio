using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

/// <summary>
/// Author: 熊昭
/// Description: 音乐板道具面板操作类
/// Date: 2021-12-03 14:51:34
/// </summary>
public class MusicBoardPanel : InfoPanel<MusicBoardPanel>,IUndoRecord
{
    public Transform areaParent;
    public Transform colorParent;

    private GameObject areaPrefab;
    private GameObject colorPrefab;

    private List<GameObject> areaSelects = new List<GameObject>();
    private List<GameObject> colorSelects = new List<GameObject>();

    private MusicBoardBehaviour mBehaviour;
    private SceneEntity curEntity;

    private int curAreaID = -1; //0 left; 1 middle; 2 right
    private int curColorID = -1; //0~36 color
    private int boardCount = 37;
    private List<int> boardColor = new List<int> { 0, 0, 0 };

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        var priAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/GameAtlas");
        areaPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "MusicBoardAreaItem");
        colorPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.PanelPath + "MusicBoardColorItem");

        for (int i = 0; i < GameManager.Inst.musicBoardDatas.Count; i++)
        {
            var musData = GameManager.Inst.musicBoardDatas[i];
            if (musData.id < boardCount)  //color
            {
                InitColorMB(priAtlas, musData);
            }
            else  //area
            {
                InitAreaMB(priAtlas, musData);
            }
        }
    }

    private void InitAreaMB(SpriteAtlas priAtlas, MusicBoardData musData)
    {
        var musGo = Instantiate(areaPrefab, areaParent);
        var selectGo = musGo.transform.GetChild(0).gameObject;
        var musImage = musGo.GetComponent<Image>();
        var musButton = musGo.GetComponent<Button>();

        selectGo.SetActive(false);
        areaSelects.Add(selectGo);
        musImage.sprite = priAtlas.GetSprite(musData.iconName);
        musButton.onClick.AddListener(() => SetMusicBoard(musData.id - 100));
    }

    private void InitColorMB(SpriteAtlas priAtlas, MusicBoardData musData)
    {
        var musGo = Instantiate(colorPrefab, colorParent);
        var selectGo = musGo.transform.GetChild(0).gameObject;
        var musImage = musGo.transform.GetChild(1).GetComponent<Image>();
        var musNum = musGo.transform.GetChild(2).GetComponent<Image>();
        var musButton = musGo.GetComponent<Button>();

        selectGo.SetActive(false);
        colorSelects.Add(selectGo);
        musImage.color = DataUtils.DeSerializeColorByHex(musData.darkColor);
        musButton.onClick.AddListener(() => SetMusicBoardAudio(musData.id));

        if (musData.id == 0)
            musNum.gameObject.SetActive(false);
        else
            musNum.sprite = priAtlas.GetSprite(musData.iconName);
    }

    public void SetEntity(SceneEntity entity)
    {
        ResBoard();

        curEntity = entity;
        var entityGo = entity.Get<GameObjectComponent>().bindGo;
        mBehaviour = entityGo.GetComponent<MusicBoardBehaviour>();
        var musComp = entity.Get<MusicBoardComponent>();

        for (int i = 0; i < boardColor.Count; i++)
        {
            boardColor[i] = musComp.audioIDs[i];
        }
        SetMusicBoard(1);
    }
    
    private void ResBoard()
    {
        if (curAreaID >= 0)
        {
            areaSelects[curAreaID].SetActive(false);
            curAreaID = -1;
        }
        if (curColorID >= 0)
        {
            colorSelects[curColorID].SetActive(false);
            curColorID = -1;
        }
    }
    public void MusicBoardUndo(int areaID,int colorID)
    {
        if (areaID!=curAreaID)
        {
            if (curAreaID >= 0)
            {
                areaSelects[curAreaID].SetActive(false);
            }
            curAreaID = areaID;
            areaSelects[curAreaID].SetActive(true);
        }
        if (curColorID >= 0)
        {
            colorSelects[curColorID].SetActive(false);
        }
        curColorID = colorID;
        colorSelects[curColorID].SetActive(true);

        if (boardColor[curAreaID] == curColorID)
            return;
        boardColor[curAreaID] = curColorID;
        mBehaviour.SetColor(curAreaID, curColorID);
        //set audio id in component by different area
        curEntity.Get<MusicBoardComponent>().audioIDs[curAreaID] = curColorID;
    }
    private void SetMusicBoard(int areaID)
    {
        if (curAreaID == areaID)
            return;
        if (curAreaID >= 0)
        {
            areaSelects[curAreaID].SetActive(false);
        }
        curAreaID = areaID;
        areaSelects[curAreaID].SetActive(true);

        SetMusicBoardAudio(boardColor[curAreaID]);
    }

    private void SetMusicBoardAudio(int colorID)
    {
        if (curColorID == colorID || curAreaID == -1)
            return;
        if (curColorID >= 0)
        {
            colorSelects[curColorID].SetActive(false);
        }

        var beginData = CreateUndoData();
        curColorID = colorID;
        colorSelects[curColorID].SetActive(true);

        if (boardColor[curAreaID] == curColorID)
            return;
        boardColor[curAreaID] = curColorID;
        mBehaviour.SetColor(curAreaID, curColorID);
       
        //set audio id in component by different area
        curEntity.Get<MusicBoardComponent>().audioIDs[curAreaID] = curColorID;
        var endData = CreateUndoData();
        AddRecord(beginData, endData);
    }
    public void AddRecord(UndoRecord record)
    {
        UndoRecordPool.Inst.PushRecord(record);
    }
    public void AddRecord(MusicBoardUndoData beginData, MusicBoardUndoData endData)
    {
        UndoRecord record = new UndoRecord(UndoHelperName.MusicBoardUndoHelper);
        record.BeginData = beginData;
        record.EndData = endData;
        AddRecord(record);
    }

    private MusicBoardUndoData CreateUndoData()
    {
        
        MusicBoardUndoData data = new MusicBoardUndoData();
        data.areaId =curAreaID;
        data.colorID = curColorID;
        data.targetEntity = curEntity;
        return data;
    }
}
