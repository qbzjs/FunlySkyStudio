using System;
using System.Collections.Generic;
using UnityEngine;


[PreferBinarySerialization]
public class MapOcclusionData : ScriptableObject
{
    [SerializeField] public string version = "1.0";

    [SerializeField] public string mapId;
    [SerializeField] public Bounds bounds;

    [SerializeField] public Vector3 chunkSize;
    [SerializeField] public Vector3 minChunkSize;

    [Serializable]
    public class WorldPos {
        public int x;
        public int y;
        public int z;

        public WorldPos(int x, int y, int z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public WorldPos(Vector3 pos, Vector3 cellSize)
        {
            x = Mathf.FloorToInt((pos.x + cellSize.x * 0.5f)/cellSize.x);
            y = Mathf.FloorToInt((pos.y + cellSize.y * 0.5f)/cellSize.y);
            z = Mathf.FloorToInt((pos.z + cellSize.z * 0.5f)/cellSize.z);
        }

        public override string ToString() {
            return $"{x},{y},{z}";
        }
    }

    [Serializable]
    public class OcclusionData
    {
        public int cellIndex;
        public int[] visibilities;
        public WorldPos worldPos;
    }

    [SerializeField] public List<OcclusionData> occlusionDataList;

    [SerializeField] public List<SpaceNodeData> spaceNodeDataList;

    private Dictionary<int, OcclusionData> occlusionDataDic = new Dictionary<int, OcclusionData>();

    private Dictionary<int, Dictionary<int, Dictionary<int, OcclusionData>>> occlusionDataWorldDic =
        new Dictionary<int, Dictionary<int, Dictionary<int, OcclusionData>>>();

    public void PreDeSerialize()
    {
        if (occlusionDataList != null)
        {
            occlusionDataDic.Clear();
            foreach (var tmpCullData in occlusionDataList)
            {
                occlusionDataDic.Add(tmpCullData.cellIndex, tmpCullData);
                if (!occlusionDataWorldDic.ContainsKey(tmpCullData.worldPos.x))
                {
                    occlusionDataWorldDic.Add(tmpCullData.worldPos.x, new Dictionary<int, Dictionary<int, OcclusionData>>());
                }

                if (!occlusionDataWorldDic[tmpCullData.worldPos.x].ContainsKey(tmpCullData.worldPos.y))
                {
                    occlusionDataWorldDic[tmpCullData.worldPos.x].Add(tmpCullData.worldPos.y, new Dictionary<int, OcclusionData>());
                }

                if (!occlusionDataWorldDic[tmpCullData.worldPos.x][tmpCullData.worldPos.y].ContainsKey(tmpCullData.worldPos.z))
                {
                    occlusionDataWorldDic[tmpCullData.worldPos.x][tmpCullData.worldPos.y].Add(tmpCullData.worldPos.z, tmpCullData);
                }
            }
        }
    }

    public OcclusionData GetOcclusionData(int index)
    {
        occlusionDataDic.TryGetValue(index, out var tmpData);
        return tmpData;
    }

    public OcclusionData GetOcclusionData(WorldPos worldPos)
    {
        if (worldPos == null)
        {
            return null;
        }

        OcclusionData data = null;
        if (!occlusionDataWorldDic.TryGetValue(worldPos.x, out var tmpDicX)) return null;
        if (tmpDicX.TryGetValue(worldPos.y, out var tmpDicY))
        {
            tmpDicY.TryGetValue(worldPos.z, out data);
        }
        return data;
    }

}