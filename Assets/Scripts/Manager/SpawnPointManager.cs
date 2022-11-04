using BudEngine.NetEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DefaultSpawnData
{
    NotDefault,
    Default
}

public class SpawnPointManager : ManagerInstance<SpawnPointManager> , IManager
{
    public List<SpawnPointBehaviour> spawnList = new List<SpawnPointBehaviour>();
    public int row = 4;
    public Action<bool> SetMaxPlayerPanelAct;
    private int _defaultSpawnId;
    public int defaultSpawnId
    {
        get
        {
            return _defaultSpawnId;
        }
        set
        {
            SetDefaultSpawnId(value);
        }
    }

    private void SetDefaultSpawnId(int value)
    {
        if(_defaultSpawnId != value)
        {
            var oldBehav = GetSpawnPointBehavById(_defaultSpawnId);
            if(oldBehav != null)
                oldBehav.anim.Play("Stop");
        }
        var newBehav = GetSpawnPointBehavById(value);
        if(newBehav != null)
            newBehav.anim.Play("DefultPoint");
        _defaultSpawnId = value;
    }

    public void Init()
    {
        MessageHelper.AddListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
    }

    public override void Release()
    {
        MessageHelper.RemoveListener<GameMode>(MessageName.ChangeMode, OnChangeMode);
        base.Release();
        Clear();
    }

    public bool IsOverMaxCount()
    {
        if (spawnList.Count >= GameConsts.MAX_PLAYER)
        {
            TipPanel.ShowToast("Oops! Exceed limit:(");
            return true;
        }
        return false;
    }

    public bool IsCanClone(GameObject curTarget)
    {
        if (!curTarget.GetComponent<SpawnPointBehaviour>())
        {
            return true;
        }
        if(IsOverMaxCount())
        {
            return false;
        }
        return true;
    }

    public void AddSpawnList(SpawnPointBehaviour spawn) 
    {
        if (!spawnList.Contains(spawn))
        {
            spawnList.Add(spawn);
        }
    }

    public void RemoveSpawnList(SpawnPointBehaviour spawn)
    {
        if (spawnList.Contains(spawn))
        {
            var defSpawn = GetSpawnPointBehavById(defaultSpawnId);
            var isEqual = spawn == defSpawn;
            spawnList.Remove(spawn);
            NumberExtension();
            defaultSpawnId = isEqual ? 1 : defSpawn.id;
        }
    }

    public void OnChangeMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Play:
                SetMaxPlayerPanelAct?.Invoke(true);
                break;
            case GameMode.Guest:
                SetMaxPlayerPanelAct?.Invoke(false);
                break;
            case GameMode.Edit:
                SceneBuilder.Inst.SpawnPoint.SetActive(true);
                var newBehav = GetSpawnPointBehavById(defaultSpawnId);
                newBehav.anim.Play("DefultPoint");
                SetMaxPlayerPanelAct?.Invoke(false);
                break;
        }
    }

    /// <summary>
    /// 创建空场景时，加载默认数量个出生点，矩阵排列
    /// </summary>
    public void OnCreateEmptyScene()
    {
        GameManager.Inst.maxPlayer = GameConsts.DEFAULT_PLAYER;
        for (int i = 0; i < GameConsts.DEFAULT_PLAYER; i++)
        {
            var newBev = SceneBuilder.Inst.CreateSceneNode<SpawnPointCreater, SpawnPointBehaviour>();
            SpawnPointCreater.SetData(newBev, null, SpawnPointCreaterType.EmptyData);
        }
        GameManager.Inst.maxPlayer = GameConsts.DEFAULT_PLAYER;
        SetDefSpawnPoint();
        MatrixArrangement(Vector3.zero);
    }
    //矩阵排列
    public void MatrixArrangement(Vector3 defOffset)
    {
        for (int i = 0; i < spawnList.Count; i++)
        {
            spawnList[i].transform.position = new Vector3(i / row, 0, i % row);
            //默认偏移值
            spawnList[i].transform.position -= new Vector3(2f - (i/row)*0.5f, 0, 2f - (i % row) * 0.5f);
            //模板地图自带偏移值
            spawnList[i].transform.position += defOffset;
        }
    }
    //重新排列出生点编号
    public void NumberExtension()
    {
        for (int i = 0; i < spawnList.Count; i++)
        {
            spawnList[i].id = i + 1;
            spawnList[i].SetNumText(spawnList[i].id);
        }
    }

    public SpawnPointBehaviour GetDefaultSpawn()
    {
        for (int i = 0; i < spawnList.Count; i++)
        {
            if (spawnList[i].id == defaultSpawnId)
            {
                return spawnList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 获得所有出生点信息，用于保存数据时使用
    /// </summary>
    /// <returns></returns>
    public SpawnData[] GetSpawnsData()
    {
        if (spawnList != null)
        {
            SpawnData[] datas = new SpawnData[spawnList.Count];
            for (int i = 0; i < spawnList.Count; i++)
            {
                SpawnData data = new SpawnData();
                data.p = DataUtils.Vector3ToString(spawnList[i].transform.localPosition);
                data.r = DataUtils.Vector3ToString(spawnList[i].transform.eulerAngles);
                data.id = spawnList[i].id;
                data.hp = spawnList[i].hpValue;
                datas[i] = data;
            }
            return datas;
        }
        return null;
    }

    /// <summary>
    /// 加载时调用
    /// </summary>
    /// <param name="mapData"></param>
    public void SetSpawnPoint(MapData mapData)
    {
        var maxPlayers = mapData.maxPlayers;
        if (maxPlayers == 0)
        {
            maxPlayers = GameConsts.DEFAULT_PLAYER;
        }
        if (mapData.spawns == null || mapData.spawns.Length <= 0)
        {
            OnCreateEmptyScene();
        }
        else
        {
            ContinueLoadingData(maxPlayers, mapData);
        }
        if (mapData.pvpData != null)
        {
            var teamInfoList = mapData.pvpData.teamList;
            SceneBuilder.Inst.UpdateBronPointTeamIDState(teamInfoList);
        }
        GameManager.Inst.maxPlayer = spawnList.Count;
    }

    private void ContinueLoadingData(int maxPlayers, MapData mapData)
    {
        SetSpawnPointByData(mapData.spawns, maxPlayers);
        defaultSpawnId = mapData.defaultSpawnId == 0 ? 1 : mapData.defaultSpawnId;
    }

    private void SetDefSpawnPoint()
    {
        for (int i = 0; i < spawnList.Count; i++)
        {
            if (spawnList[i].id == 1)
            {
                defaultSpawnId = spawnList[i].id;
                return;
            }
        }
    }

    /// <summary>
    /// 获得当前玩家的出生点
    /// </summary>
    /// <returns></returns>
    public GameObject GetSpawnPoint()
    {
        int id = GameManager.Inst.PlayerSpawnId > 0 ? (GameManager.Inst.PlayerSpawnId - 1) : 0;
        if (spawnList != null && spawnList.Count > id && spawnList[id] != null)
        {
            switch (GlobalFieldController.CurGameMode)
            {
                case GameMode.Play:
                    var defaultSpawn = GetDefaultSpawn();
                    if (defaultSpawn != null)
                    {
                        return defaultSpawn.gameObject;
                    }
                    else
                    {
                        return spawnList[id].gameObject;
                    }
                case GameMode.Guest:
                    return spawnList[id].gameObject;
            }
        }
        return spawnList[0].gameObject;
    }

    public SpawnPointBehaviour GetSpawnPointBehavByGameMode(int spawnId)
    {
        switch (GlobalFieldController.CurGameMode)
        {
            case GameMode.Play:
                var defaultSpawn = GetDefaultSpawn();
                if (defaultSpawn != null)
                {
                    return defaultSpawn;
                }
                break;
            case GameMode.Guest:
                for (int i = 0; i < spawnList.Count; i++)
                {
                    if (spawnList[i].id == spawnId)
                    {
                        return spawnList[i];
                    }
                }
                break;
        }
        return spawnList[0];
    }

    public SpawnPointBehaviour GetSpawnPointBehavById(int spawnId)
    {
        if(spawnList.Count <= 0)
        {
            return null;
        }
        for (int i = 0; i < spawnList.Count; i++)
        {
            if (spawnList[i].id == spawnId)
            {
                return spawnList[i];
            }
        }
        return spawnList[0];
    }

    public bool IsCanDesTarget()
    {
        if (spawnList.Count > 1)
        {
            return true;
        }
        TipPanel.ShowToast("At least 1 player");
        return false;
    }

    public int GetPlayerSpawnId(string playerId)
    {
        if(Global.Room != null)
        {
            var globalList = Global.Room.RoomInfo.PlayerList;
            for (int i = 0; i < globalList.Count; i++)
            {
                if(playerId == globalList[i].Id)
                {
                    return globalList[i].Spawn;
                }
            }
        }
        return 1;
    }

    /// <summary>
    /// Downtown设置出生点信息
    /// </summary>
    /// <param name="spawns"></param>
    public void SetSpawnPointByData(SpawnData[] spawns, int maxPlayers = 16)
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            if (i < spawns.Length)
            {
                var newBev = SceneBuilder.Inst.CreateSceneNode<SpawnPointCreater, SpawnPointBehaviour>();
                SpawnPointCreater.SetData(newBev, spawns[i], SpawnPointCreaterType.ContinueLoadingData);
            }
        }
    }

    public void Clear()
    {
        spawnList.Clear();
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.BornPoint)
        {
            RemoveSpawnList(behaviour as SpawnPointBehaviour);
            GameManager.Inst.maxPlayer = spawnList.Count;
            if (PVPTeamManager.Inst.IsTeamMode())
            {
                PVPTeamManager.Inst.UpdateTeamInfo();
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.BornPoint)
        {
            SpawnPointBehaviour sbehav = behaviour as SpawnPointBehaviour;
            AddSpawnList(sbehav);
            GameManager.Inst.maxPlayer = spawnList.Count;
            sbehav.SetNumText(spawnList.Count);
            if (PVPTeamManager.Inst.IsTeamMode() && spawnList.Count < GameConsts.MAX_PLAYER)
            {
                PVPTeamManager.Inst.UpdateTeamInfo();
            }
        }
    }
}
