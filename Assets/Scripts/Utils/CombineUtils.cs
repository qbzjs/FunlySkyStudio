using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Assets.Scripts.Game.Core;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

/// <summary>
/// Author:YangJie
/// Description:
/// Date: #CreateTime#
/// </summary>
public class CombineUtils
{
    private static string cacheFolder = "";
    public static string CacheFolder
    {
        get
        {
            if (string.IsNullOrEmpty(cacheFolder))
            {
                cacheFolder = Path.Combine(DataUtils.dataDir, "Mesh/") ;
            }
            return cacheFolder;
        }

        set => cacheFolder = value;
    }
    private static Dictionary<int, Mesh> meshDic = new Dictionary<int, Mesh>();
    private static MaterialPropertyBlock defaultMpb = new MaterialPropertyBlock();
    public static GameObject DeSerializeNodeData(Transform parent, NodeData nodeData, bool isCacheRoot = true)
    {
        var rid = nodeData.rid;
        var ugcCacheObj = new GameObject("UGCAssetObj");

        ugcCacheObj.transform.localScale = Vector3.one;
        ugcCacheObj.transform.localPosition = Vector3.zero;
        ugcCacheObj.transform.localRotation = Quaternion.identity;
        if (!Directory.Exists(CacheFolder))
        {
            Directory.CreateDirectory(CacheFolder);
        }
        GetCombineNodeData(nodeData.prims, out var combineNodes, out var unCombineNodes);
        CreateCombineMesh(rid, ugcCacheObj, combineNodes);
        SceneBuilder.Inst.BuildAllNodes(unCombineNodes, isCacheRoot ? ugcCacheObj.transform : parent);
        var nodeBehaviours = ugcCacheObj.GetComponentsInChildren<NodeBaseBehaviour>(true);
        foreach (var tmpNodeBehaviour in nodeBehaviours)
        {
            if (tmpNodeBehaviour is DTextBehaviour || tmpNodeBehaviour is NewDTextBehaviour || tmpNodeBehaviour is PGCBehaviour)
            {
                SceneBuilder.Inst.RemoveNodeBehaviour(tmpNodeBehaviour);
                Object.DestroyImmediate(tmpNodeBehaviour);
            }
        }
        ugcCacheObj.layer = LayerMask.NameToLayer("Model");
        ugcCacheObj.transform.SetParent(parent);
        return ugcCacheObj;
    }
    
    private static void CreateCombineMesh(string rid, GameObject ugcCacheObj, List<NodeData> prims)
    {
        var newMeshList = new List<Mesh>();
        Dictionary<string, List<CombineInstance>> meshCombines = new Dictionary<string, List<CombineInstance>>();
        foreach (var tmpData in prims)
        {
            var colorMatData = GameUtils.GetAttr<ColorMatData>((int) BehaviorKey.ColorMaterial, tmpData.attr);
            if (colorMatData == null)
            {
                colorMatData = new ColorMatData()
                {
                    cols = DataUtils.ColorToString(Color.white),
                    tile = DataUtils.Vector2ToString(Vector2.one),
                    mat = 0
                };
            }
            
            if (colorMatData.mat == 0 || colorMatData.mat == 23 || colorMatData.mat == 1)
            {
                colorMatData.tile = DataUtils.Vector2ToString(Vector2.one);
            }
            var key = $"{colorMatData.cols}_{colorMatData.tile}_{colorMatData.mat}";
            if(!string.IsNullOrEmpty(colorMatData.umat))
            {
                key = $"{colorMatData.cols}_{colorMatData.tile}_{colorMatData.mat}_{colorMatData.umat}";
            }
            if (!meshCombines.ContainsKey(key))
            {
                meshCombines.Add(key, new List<CombineInstance>());
            }
            var rot = DataUtils.DeSerializeVector3(tmpData.r);
            var sca = DataUtils.DeSerializeVector3(tmpData.s);
            Vector3 pos = DataUtils.DeSerializeVector3(tmpData.p);
            sca = DataUtils.LimitVector3(sca);
            var combineInstance = new CombineInstance();
            combineInstance.mesh = GetMesh(tmpData.id); //将共享mesh，赋值
            combineInstance.transform = Matrix4x4.TRS(pos,  Quaternion.Euler(rot),  sca);
            meshCombines[key].Add(combineInstance);
        }

        foreach (var keyValue in meshCombines)
        {
            var tmpMesh = new Mesh()
            {
                name = keyValue.Key
            };
            newMeshList.Add(tmpMesh);
            var verticesCount = 0;
            foreach (var tmpCombineInstance in keyValue.Value)
            {
                verticesCount += tmpCombineInstance.mesh.vertices.Length;
            }
            if (verticesCount >= 65535)
            {
                tmpMesh.indexFormat = IndexFormat.UInt32;
            }

            tmpMesh.CombineMeshes(keyValue.Value.ToArray()); //将combineInstances数组传入函数

            CreateObj(tmpMesh, ugcCacheObj.transform);
        }
    }
    private static void ParseMeshObj(GameObject meshObj, bool isHasTexture = true)
    {
        meshObj.TryGetComponent<MeshRenderer>(out var meshRenderer);
        if (meshRenderer == null)
        {
            meshRenderer = meshObj.AddComponent<MeshRenderer>();
        }
        if (!meshObj.TryGetComponent<MeshCollider>(out var _))
        {
            meshObj.AddComponent<MeshCollider>();
        }
        
        var tmpMesh = meshObj.GetComponentInChildren<MeshFilter>();
        var matValues = tmpMesh.name.Split('_');
        var color = DataUtils.DeSerializeColor(matValues[0]);
        var tile = DataUtils.DeSerializeVector2(matValues[1]);
        var mapId = int.Parse(matValues[2]);
        var matData = GameManager.Inst.matConfigDatas.Find(x => x.id == mapId);
        if (matData == null)
        {
            return;
        }
        var mat = matData.id != 1 ? GameManager.Inst.BaseModelMats[0] : GameManager.Inst.BaseModelMats[1];
        meshRenderer.material = mat;
        meshRenderer.material.name = $"Ground_{matData.id} (Instance)";
        if (mapId == 1)
        {
            color.a = 0.36f;
        }
        defaultMpb.Clear();
        defaultMpb.SetColor("_Color", color);
        defaultMpb.SetFloat("_Glossiness", matData.smoothness);
        defaultMpb.SetFloat("_Metallic", matData.metallic);
        if (matData.id == 0 || matData.id == 23 || matData.id == 1)
        {
            tile = Vector2.one;
        }
        defaultMpb.SetVector("_MainTex_ST", new Vector4(tile.x, tile.y, 0, 0));
        var tex = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName);
        var normalMap = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + matData.texName + "_normal");
        if (normalMap == null)
        {
            normalMap = ResManager.Inst.LoadRes<Texture>(GameConsts.BaseTexPath + "default_normal");
        }
        defaultMpb.SetTexture("_MainTex", tex);
        if(matValues.Length > 3)
        {
            Texture tmpTex = null;
            UGCTexManager.Inst.GetUGCTexWithUMat(matValues[3],(texture)=>{
                tmpTex = texture;
                defaultMpb.SetTexture("_MainTex", tmpTex);
                defaultMpb.SetTexture("_BumpMap", normalMap);
                meshRenderer.SetPropertyBlock(defaultMpb);
            });
        }
        else
        {
            defaultMpb.SetTexture("_BumpMap", normalMap);
            meshRenderer.SetPropertyBlock(defaultMpb);
        }
        
    }

    private static void GetCombineNodeData(List<NodeData> prims, out List<NodeData> combineNodes, out List<NodeData> unCombineNodes)
    {

        unCombineNodes = new List<NodeData>();
        combineNodes = new List<NodeData>();
        foreach (var tmpNode in prims)
        {
            if (tmpNode.type == (int)ResType.Single)
            {
                if (GameManager.Inst.priConfigData.TryGetValue(tmpNode.id, out var configData))
                {
                    if ((NodeModelType) configData.modType != NodeModelType.BaseModel)
                    {
                        unCombineNodes.Add(tmpNode);
                    }
                    else
                    {
                        var tmpMesh = GetMesh(tmpNode.id);
                        if (tmpMesh.isReadable)
                        {
                            combineNodes.Add(tmpNode);
                        }
                        else
                        {
                            unCombineNodes.Add(tmpNode);
                        }
                    }
                }
                else
                {
                    unCombineNodes.Add(tmpNode);
                    Debug.LogError("configData == null:" + tmpNode.id);
                }
            }
            else
            {
                unCombineNodes.Add(tmpNode);
            }
        }
    }

    private static Mesh GetMesh(int id)
    {
        if (!meshDic.ContainsKey(id))
        {
            var modelData = GameManager.Inst.priConfigData[id];
            var assetPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.BaseModelPath + modelData.prefabName);//实例化预制体
            meshDic.Add(id, assetPrefab.GetComponentInChildren<MeshFilter>().sharedMesh);
        }

        return meshDic[id];
    }

    private static GameObject CreateObj(Mesh mesh, Transform parent)
    {
        var tmpObj = new GameObject(mesh.name);
        tmpObj.layer = LayerMask.NameToLayer("Model");
        tmpObj.transform.SetParent(parent);
        tmpObj.transform.localPosition = Vector3.zero;
        tmpObj.transform.localRotation = Quaternion.identity;
        tmpObj.transform.localScale = Vector3.one;
        tmpObj.AddComponent<MeshFilter>().sharedMesh = mesh; //给当前空物体，添加网格组件；将合并后的网格，给到自身网格
        ParseMeshObj(tmpObj);
        return tmpObj;
    }

    private static string GetRid(List<NodeData> prims)
    {
        var hashes = new List<string>();
        if (prims == null || prims.Count == 0) return HashUtils.HashString(hashes);
        foreach (var tmpNodeData in prims)
        {
            tmpNodeData.uid = 0;
            hashes.Add(tmpNodeData.ToHash());
        }
        return HashUtils.HashString(hashes);
    }

    public static void Clear()
    {
        meshDic.Clear();
        defaultMpb.Clear();
    }
}
