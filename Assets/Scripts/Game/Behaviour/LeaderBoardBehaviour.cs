using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
/// <summary>
/// Author:LiShuZhan
/// Description:排行榜脚本，处理展示的逻辑
/// Date: 2022.04.14
/// </summary>
public class LeaderBoardBehaviour : NodeBaseBehaviour
{
    public Dictionary<int, SceneEntity> playerInfos = new Dictionary<int, SceneEntity>();
    public Transform editPanel;
    public Transform playPanel;
    public Transform itemPanel;
    private Transform notRecordText;
    public List<GameObject> itemLists = new List<GameObject>();

    private Vector3 farstPos = new Vector3(-0.11f, 0.31f, -0.01f);
    private Vector3 secondPos = new Vector3(-0.11f, 0.14f, -0.01f);
    private Vector3 thirdPos = new Vector3(-0.11f, -0.03f, -0.01f);
    private Vector3 fourthPos = new Vector3(-0.11f, -0.2f, -0.01f);
    private Vector3 fifthPos = new Vector3(-0.11f, -0.375f, -0.01f);
    public Vector3 size = new Vector3(0.2f, 0.2f, 0.2f);
    public List<Vector3> posList = new List<Vector3>();
    private Coroutine GetPhotoCor;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        editPanel = transform.Find("EditMode");
        playPanel = transform.Find("PlayMode");
        itemPanel = playPanel.Find("LeaderBoardItem");
        notRecordText = playPanel.Find("NotRecords");
        posList.Add(farstPos);
        posList.Add(secondPos);
        posList.Add(thirdPos);
        posList.Add(fourthPos);
        posList.Add(fifthPos);
        if (GetPhotoCor != null)
        {
            StopCoroutine(GetPhotoCor);
            GetPhotoCor = null;
        }
    }

    public void ChangeGameMode(GameMode gameMode)
    {
        if (entity.Get<LeaderBoardComponent>().curMode == (int)LeaderBoardModeType.None)
        {
            return;
        }
        bool active = gameMode == GameMode.Edit ? false : true;
        editPanel.gameObject.SetActive(!active);
        playPanel.gameObject.SetActive(active);
        if (active == false)
        {
            ClearList();
        }
    }

    public void ClearList()
    {
        itemLists.ForEach(x => Destroy(x));
        itemLists.Clear();
    }
}
