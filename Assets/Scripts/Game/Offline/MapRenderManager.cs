using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Collections;
using HLODSystem;
using HLODSystem.SpaceManager;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MapRenderManager : ManagerInstance<MapRenderManager>, IManager
{
    private MapOcclusionData mapOcclusionData;
    private Dictionary<string, BaseHLODBehaviour> hlodBehaviourDic = new Dictionary<string, BaseHLODBehaviour>();
    private Dictionary<int, SpaceNode> renderSpaceDic = new Dictionary<int, SpaceNode>();
    private HashSet<int> renderSpaceNodes = new HashSet<int>();
    private AssetBundleLoader mapOcclusionDataLoader = null;
    private MapRenderInfo mapRenderInfo;
    private List<MapOcclusionData.OcclusionData> tmpOcclusionDataList = new List<MapOcclusionData.OcclusionData>();

    private Matrix4x4 cameraMatrix4X4 = new Matrix4x4(new Vector4(0.5625f, 0, 0, 0), new Vector4(0, 1f, 0, 0),
        new Vector4(0, 0, -1.00020f, -1f), new Vector4(0, 0, -0.20002f, 0));

    private Vector3[] samples = new Vector3[]
    {
        new Vector3(1, 0, 0),
        new Vector3(1, 0, 1),
        new Vector3(0, 0, 1),
        new Vector3(-1, 0, 1),
        new Vector3(-1, 0, 0),
        new Vector3(-1, 0, -1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, -1),

        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
        new Vector3(-1, 1, 1),
        new Vector3(-1, 1, 0),
        new Vector3(-1, 1, -1),
        new Vector3(0, 1, -1),
        new Vector3(1, 1, -1),

        new Vector3(0, -1, 0),
        new Vector3(1, -1, 0),
        new Vector3(1, -1, 1),
        new Vector3(0, -1, 1),
        new Vector3(-1, -1, 1),
        new Vector3(-1, -1, 0),
        new Vector3(-1, -1, -1),
        new Vector3(0, -1, -1),
        new Vector3(1, -1, -1),
    };

    private Vector3 nodeBaseSize = Vector3.one;
    public bool isOcclusionEnable = false;
    public int occlusionCount = 0;

    public void Init(List<MapRenderInfo> renderInfos = null)
    {
        mapRenderInfo = FilterMapRenderInfo(renderInfos);
        if (GlobalFieldController.CurGameMode != GameMode.Guest || mapRenderInfo == null)
        {
            Clear();
            return;
        }

        InitOcclusion();
        Camera.onPreCull -= OnCameraPreCull;
        Camera.onPreCull += OnCameraPreCull;
    }

    private int lastPosIndex = -1;
    private MapOcclusionData.WorldPos lastWorldPos = null;

    private void OnCameraPreCull(Camera cam)
    {
        UpdateCalculateOcclusion(cam);
    }


    private void InitOcclusion()
    {
        if (mapRenderInfo == null || string.IsNullOrEmpty(mapRenderInfo.occlusionUrl))
        {
            return;
        }

        mapOcclusionDataLoader = new AssetBundleLoader(mapRenderInfo.occlusionUrl, (action, asset, err) =>
        {
            if (string.IsNullOrEmpty(err) && asset != null)
            {
                var assetBundle = asset as AssetBundle;
                if (assetBundle != null)
                {
                    mapOcclusionData = assetBundle.LoadAsset<MapOcclusionData>("MapOcclusionData");
                    assetBundle.Unload(false);
                }
                if (mapOcclusionData != null)
                {
                    LRUManager<MapOfflineLRUInfo>.Inst.Put(new MapOfflineLRUInfo()
                    {
                        key = mapOcclusionData.mapId + "_" + "occlusion",
                        cacheFilePath = mapOcclusionDataLoader.FileName
                    });
                    mapOcclusionData.PreDeSerialize();
                    SplitMap();
                }
                else
                {
                    LoggerUtils.LogError("mapRenderData == null");
                }
            }

            mapOcclusionDataLoader.Dispose();
            mapOcclusionDataLoader = null;
        });

        mapOcclusionDataLoader.SaveFolder =
            Path.Combine(Application.persistentDataPath, GameConsts.OfflineCachePath, "Map");
        mapOcclusionDataLoader.Do();
    }

    /// <summary>
    /// 更新遮挡剔除
    /// </summary>
    /// <param name="cam"></param>
    private void UpdateCalculateOcclusion(Camera cam)
    {
        if (!cam.CompareTag("MainCamera") || !isOcclusionEnable) return;
        var cameraPos = cam.transform.position;
        var worldPos = new MapOcclusionData.WorldPos(cameraPos, nodeBaseSize);
        if (lastWorldPos != null && worldPos.x == lastWorldPos.x && worldPos.y == lastWorldPos.y && worldPos.z == lastWorldPos.z)
        {
            return;
        }

        lastWorldPos = worldPos;
        tmpOcclusionDataList.Clear();

        var tmpOcclusionData = mapOcclusionData.GetOcclusionData(worldPos);
        if (tmpOcclusionData != null)
        {
            tmpOcclusionDataList.Add(tmpOcclusionData);
        }
        
        // 整队相机周围一圈(26个点) 进行采样，获取遮挡剔除数据
        foreach (var tmpSample in samples)
        {
            var tmpPos = cameraPos + new Vector3(tmpSample.x * nodeBaseSize.x, tmpSample.y * nodeBaseSize.y,
                tmpSample.z * nodeBaseSize.z);
            var tmpWorldPos = new MapOcclusionData.WorldPos(tmpPos, nodeBaseSize);
            tmpOcclusionData = mapOcclusionData.GetOcclusionData(tmpWorldPos);
            if (tmpOcclusionData != null)
            {
                tmpOcclusionDataList.Add(tmpOcclusionData);
            }
        }

        if (tmpOcclusionDataList.Count == 0)
        {
            QueueToggleAllSpaceNodes(true);
        }
        else
        {
            QueueToggleAllSpaceNodes(false);
            foreach (var occlusionData in tmpOcclusionDataList)
            {
                foreach (var visibleIndex in occlusionData.visibilities)
                {
                    if (!renderSpaceNodes.Contains(visibleIndex))
                    {
                        renderSpaceNodes.Add(visibleIndex);
                    }
                }
            }
        }

        ExecuteQueue();
    }

    public void QueueToggleAllSpaceNodes(bool state)
    {
        if (!state)
        {
            renderSpaceNodes.Clear();
            return;
        }

        foreach (var keyValuePair in renderSpaceDic)
        {
            renderSpaceNodes.Add(keyValuePair.Key);
        }
    }

    private void ExecuteQueue()
    {
        occlusionCount = 0;
        foreach (var keyValuePair in renderSpaceDic)
        {
            var isVisible = renderSpaceNodes.Contains(keyValuePair.Key);
            keyValuePair.Value.ToggleRender(isVisible);
            if (!isVisible && FPSPanel.Instance.isActive)
            {
                foreach (var tmpBehaviour in keyValuePair.Value.Behaviours)
                {
                    if (tmpBehaviour.hlodState != HLODState.Cull)
                    {
                        occlusionCount += 1;
                    }
                }
            }
        }
    }

    public void AddBehaviour(NodeBaseBehaviour nodeBehaviour)
    {
        if (nodeBehaviour != null && nodeBehaviour is BaseHLODBehaviour behaviour)
        {
            if (!string.IsNullOrEmpty(behaviour.HLODID) && !hlodBehaviourDic.ContainsKey(behaviour.HLODID))
            {
                hlodBehaviourDic.Add(behaviour.HLODID, behaviour);
            }
        }
    }


    public BaseHLODBehaviour GetBehaviour(string hlodID)
    {
        if (string.IsNullOrEmpty(hlodID))
        {
            return null;
        }

        hlodBehaviourDic.TryGetValue(hlodID, out var behaviour);
        return behaviour;
    }

    // 解析分块数据，并重建分块数据
    public void SplitMap()
    {
        foreach (var spaceNodeData in mapOcclusionData.spaceNodeDataList)
        {
            var tmpSpaceNode = SpaceNode.CreateSpaceNodeWithBounds(spaceNodeData.bounds);
            tmpSpaceNode.index = spaceNodeData.index;
            foreach (var nodeId in spaceNodeData.nodes)
            {
                if (hlodBehaviourDic.TryGetValue(nodeId, out var behaviour))
                {
                    tmpSpaceNode.AddBehaviour(behaviour);
                }
            }
            renderSpaceDic.Add(tmpSpaceNode.index, tmpSpaceNode);
            if (renderSpaceDic.TryGetValue(spaceNodeData.parent, out var tmpParentSpaceNode))
            {
                tmpSpaceNode.ParentNode = tmpParentSpaceNode;
            }
        }
        nodeBaseSize = mapOcclusionData.minChunkSize;
        isOcclusionEnable = true;
    }

    public void RemoveNode(NodeBaseBehaviour nodeBehaviour)
    {
        if (nodeBehaviour != null && nodeBehaviour is BaseHLODBehaviour behaviour)
        {
            if (!string.IsNullOrEmpty(behaviour.HLODID) && hlodBehaviourDic.ContainsKey(behaviour.HLODID))
            {
                hlodBehaviourDic.Remove(behaviour.HLODID);
            }
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
    }

    private MapRenderInfo FilterMapRenderInfo(List<MapRenderInfo> renderInfos)
    {
        // 需要限定版本，发布之后需要重新渲染
        var tmpRenderInfo = renderInfos?.FirstOrDefault(tmp => tmp.platform == 
            Application.platform && tmp.version == MapRenderInfo.V10 
                                 && tmp.renderTime > GlobalFieldController.CurMapInfo.lastModifiedTime);
        return tmpRenderInfo;
    }

    public void Clear()
    {
        mapOcclusionData = null;
        occlusionCount = 0;
        hlodBehaviourDic.Clear();
        renderSpaceDic.Clear();
        renderSpaceNodes.Clear();
        isOcclusionEnable = false;
        if (mapOcclusionDataLoader != null)
        {
            mapOcclusionDataLoader.Dispose();
            mapOcclusionDataLoader = null;
        }

        mapRenderInfo = null;
        MapNodeData.Clear();
        Camera.onPreCull -= OnCameraPreCull;
    }

    public override void Release()
    {
        base.Release();
        Clear();
    }
}