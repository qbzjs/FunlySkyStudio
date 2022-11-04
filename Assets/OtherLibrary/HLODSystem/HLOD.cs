/// <summary>
/// Author: YangJie
/// Description:
/// Date: 2022/5/16 13:7:38
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cinemachine;
using HLODSystem.Controller;
using HLODSystem.Trees;
using UnityEngine;
using UnityEngine.Rendering;

namespace HLODSystem
{
    public class HLOD : MonoManager<HLOD>
    {
        private DefaultHLODController m_hlodController;
        private string curMapId;

        private Dictionary<string, BaseHLODBehaviour> m_behaviourDic = new Dictionary<string, BaseHLODBehaviour>();

        private bool isStart = false;
        public bool IsCameraCull { get; set; } = true;

        public bool IsValid => m_hlodController != null;
        public DefaultHLODController HLODController => m_hlodController;
        public Action<BaseHLODBehaviour> OnHLODBehaviourStatusChange;
        
        public void AddEntity(SceneEntity entity, NodeBaseBehaviour behaviour) {
            if (!(behaviour is BaseHLODBehaviour hlodBehaviour)) {
                LoggerUtils.Log("AddEntity NOT BaseHLODBehaviour:" );
                return;
            }

            if (m_hlodController == null || hlodBehaviour.IsMoving(hlodBehaviour)) {
                hlodBehaviour.SetLODStatus(HLODState.High);
                return;
            }

            var mComp = entity.Get<HLODComponent>();
            var treeNode = GetTreeNode(mComp.hlodId);
            if (treeNode != null)
            {
                treeNode.AddBehaviour(mComp.hlodId, hlodBehaviour);
            }
            else
            {
                hlodBehaviour.SetLODStatus(HLODState.High);
            }
        }


        public void LoadMapOfflineData(string mapId, Action callBack)
        {
            isStart = false;
            UGCBehaviorManager.Inst.InitOfflineRenderData();
            GlobalFieldController.offlineRenderDataDic.TryGetValue(mapId, out var renderData);
            if (renderData == null) {
                callBack?.Invoke();
                return;
            }

            if (curMapId != mapId && m_hlodController != null) {
                GameObject.Destroy(m_hlodController.gameObject);
                m_hlodController = null;
            } else if (curMapId == mapId && m_hlodController != null) {
                callBack?.Invoke();
                return;
            }
            LoggerUtils.Log("#######进房流程 开始加载 地图渲染数据:" + Path.GetFileName(renderData.GetRenderUrl()));
            DataLogUtils.LogUnityGetMapOfflineABReq();
            var loader = new AssetBundleLoader(renderData.GetRenderUrl(), (action, asset, err) =>
            {
                bool isSuccess = false;
                if (string.IsNullOrEmpty(err) && asset != null)
                {
                    isSuccess = true;
                    var tmpObj = ((AssetBundle)asset).LoadAsset<GameObject>("HLODRoot");
                    if (tmpObj != null) {
                        m_hlodController = Instantiate(tmpObj, transform).GetComponent<DefaultHLODController>();
                        m_hlodController.Init();
                        Camera.onPreCull -= OnPreCullCallBack;
                        Camera.onPreCull += OnPreCullCallBack;
                        DestroyImmediate(tmpObj, true);
                    }
                    if(isSuccess && renderData.GetCachePath() != null)
                    {
                        FileInfo fileInfo = new FileInfo(renderData.GetCachePath());
                        FileLRUInfo fileLRUInfo = new FileLRUInfo
                        {
                            key = mapId,
                            cacheFilePath = renderData.GetFileName(),
                            size = (ulong)fileInfo.Length
                        };
                        OfflineResManager.Inst.AddAbFile(fileLRUInfo);
                    }
                    ((AssetBundle) asset).Unload(false);
                }
                DataLogUtils.LogUnityGetMapOfflineABRsp(isSuccess ? "0" : "1");
                action.Dispose(); 
                callBack?.Invoke();
            }) {
                SaveFolder = Application.persistentDataPath + "/" + GameConsts.OfflineCachePath,
            };
            loader.Do();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }

        public List<string> GetHighUGCRids(MapData mapData)
        {
            if (m_hlodController == null)
            {
                return null;
            }

            float lodDis = GameConsts.DefaultMapBlock.spawnLodDistance;
            if (GameManager.Inst.unityConfigInfo.mapBlock != null)
            {
                lodDis = GameManager.Inst.unityConfigInfo.mapBlock.spawnLodDistance;
            }
            m_hlodController.LODDistance = lodDis;
            var spawnPos = SpawnPointManager.Inst.GetSpawnPoint();
            var rids = new List<string>();
            GetHighUGCRids(spawnPos.transform.localPosition, spawnPos.transform.localEulerAngles, mapData, rids);
            if (mapData.pvpData != null)
            {
                var pos = DataUtils.DeSerializeVector3(mapData.pvpData.p);
                var rot = DataUtils.DeSerializeVector3(mapData.pvpData.r);
                GetHighUGCRids(pos, rot, mapData, rids);
            }
            rids = rids.Distinct().ToList();
            return rids;
        }

        public void GetHighUGCRids(Vec3 pos, Vec3 rot, MapData mapData, List<string> rids)
        {
            SetHLODCameraPos(pos, rot);
            m_hlodController.UpdateCull(HLODCameraRecognizer.RecognizedCamera);
            var highTreeNodes = m_hlodController.GetHighTreeNodes();
            foreach (var childNodeData in mapData.pref)
            {
                GetHighUGCRids(childNodeData, highTreeNodes, rids);
            }
        }

        public void GetHighUGCRids(NodeData nodeData, List<HLODTreeNode> hlodTreeNodes, List<string> rids)
        {
            if (hlodTreeNodes == null)
            {
                return;
            }
            var rType = (ResType) nodeData.type;
            if (rType == ResType.CommonCombine)
            {
                if (nodeData.prims != null)
                {
                    foreach (var childNodeData in nodeData.prims)
                    {
                        GetHighUGCRids(childNodeData, hlodTreeNodes, rids);
                    }
                }
            }
            else if (rType == ResType.UGC)
            {
                foreach (var tmpTreeNode in hlodTreeNodes)
                {
                    if (tmpTreeNode.IsContains(nodeData.uid + "_" + nodeData.ToHash()))
                    {
                        rids.Add(nodeData.rid);
                        break;
                    }
                }
            }
        }

        public void ResetController()
        {
            if (m_hlodController != null)
            {
                if (!isStart)
                {
                    isStart = true;
                }
                m_hlodController.UpdateCull(HLODCameraRecognizer.RecognizedCamera);
            }
        }

        public void StopController()
        {
            isStart = false;
        }


        public bool IsContain(string uidHash)
        {
            if (m_hlodController == null)
            {
                return false;
            }

            return m_hlodController.Root.ObjectIds.Contains(uidHash) ||
                   m_hlodController.Container.TreeNodes.Any(treeNode => treeNode != null && treeNode.ObjectIds.Contains(uidHash));
        }

        public HLODTreeNode GetTreeNode(string hlodId)
        {
            if (m_hlodController == null)
            {
                return null;
            }
            return m_hlodController.Container.TreeNodes.Find(treeNode => treeNode != null && treeNode.ObjectIds.Contains(hlodId) );
        } 

        public BaseHLODBehaviour GetBehaviour(string uniqueId)
        {
            return m_behaviourDic.ContainsKey(uniqueId) ? m_behaviourDic[uniqueId] : null;
        }
        

        private void Clear()
        {
            m_behaviourDic?.Clear();
            isStart = false;
            OnHLODBehaviourStatusChange = null;
            Camera.onPreCull -= OnPreCullCallBack;
        }

        private void OnPreCullCallBack(Camera cam)
        {
            if (!isStart)
            {
                return;
            }
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == false)
            {
                if (UnityEditor.SceneView.currentDrawingSceneView == null)
                    return;
                if (cam != UnityEditor.SceneView.currentDrawingSceneView.camera)
                    return;
            }
            else
            {
                if (cam != HLODCameraRecognizer.RecognizedCamera)
                    return;
            }
#else
            if (cam != HLODCameraRecognizer.RecognizedCamera)
                return;
#endif
            if (m_hlodController == null)
            {
                return;
            }

            m_hlodController.UpdateCull(cam);
        }

        private void SetHLODCameraPos(Vec3 pos, Vec3 rot)
        {
            PlayerControlManager.Inst.playerBase.transform.SetPositionAndRotation(pos + PlayerControlManager.Inst.playerBase.initPos, Quaternion.Euler(rot));
            var worldPos = PlayerControlManager.Inst.playerBase.transform.TransformPoint(new Vector3(0, 0.7f, -7));
            HLODCameraRecognizer.RecognizedCamera.transform.SetPositionAndRotation(worldPos, Quaternion.Euler(rot));
        }
    }
}
