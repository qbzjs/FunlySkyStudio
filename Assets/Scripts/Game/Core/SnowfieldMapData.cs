using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SnowfieldMapData
{
    public int version;
    public int worldWidth = 500;
    public int worldHeight = 500;
    public int regionWidth = 100;
    public int regionHeight = 100;
    public int maxPlayers = 16;
    public SpawnData[] spawns;//spawn array instead of "pspawn"
    public SkyData sky;//skybox type index
    public DirLightData dir; //direct light
    public GameTerrainData ter;//PGC基础地形信息
    public BGMusicData bgmusic;
    public List<NodeData> pref = new List<NodeData>();
    public PostProcessData postprocess;
    public WeatherSaveParams weather;
    public List<string> subMaps = new List<string>();
}

