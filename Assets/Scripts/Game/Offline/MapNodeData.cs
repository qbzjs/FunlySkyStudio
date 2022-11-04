using System.Collections.Generic;
using Assets.Scripts.Game.Core;
using HLODSystem.Extensions;
using UnityEngine;

public class MapNodeData
{

    public string hash;
    public NodeData nodeData;
    public MapNodeData Parent { get; set; }
    public List<MapNodeData> Children { get; set; }

    private static Dictionary<string, MapNodeData> mapNodeDataDic = new Dictionary<string, MapNodeData>();
    private static Dictionary<int, Bounds> nodeBounds = new Dictionary<int, Bounds>();
    private static Dictionary<string, Bounds?> ugcNodeBounds = new Dictionary<string, Bounds?>();
    private static MapData curMapData = null;
    public void Setup()
    {
        if (mapNodeDataDic.ContainsKey(hash))
        {
            LoggerUtils.Log("ContainsKey:" + nodeData.uid);
            return;
        }
        else
        {
            mapNodeDataDic.Add(hash, this);
        }
        if (nodeData == null) return;
        foreach (var tmpPrim in nodeData.prims)
        {
            var tmpMapNode = new MapNodeData()
            {
                nodeData = tmpPrim,
                hash = tmpPrim.ToHash(),
                Parent = this,
                Children = new List<MapNodeData>()
            };
            Children.Add(tmpMapNode);
            tmpMapNode.Setup();
        }
    }

    private Bounds? bounds;

    public Bounds? GetBounds()
    {
        if (bounds == null)
        {
            var tmpBounds = BaseBounds();
            if (tmpBounds != null)
            {
                var tmpParent = this;
                while (tmpParent != null)
                {
                    var tmpPos = DataUtils.DeSerializeVector3(tmpParent.nodeData.p);
                    var tmpRot = DataUtils.DeSerializeVector3(tmpParent.nodeData.r);
                    var tmpScale = DataUtils.DeSerializeVector3(tmpParent.nodeData.s);

                    tmpBounds = tmpBounds.Value.TransformBounds(Matrix4x4.TRS(tmpPos, Quaternion.Euler(tmpRot),
                        tmpScale));
                    tmpParent = tmpParent.Parent;
                }
            }

            bounds = tmpBounds;
        }

        return bounds;
    }

    private Bounds? baseBounds = null;

    private Bounds? BaseBounds()
    {
        if (baseBounds == null)
        {
            if (Children.Count == 0)
            {
                if (!string.IsNullOrEmpty(nodeData.rid))
                {
                    if (ugcNodeBounds.ContainsKey(nodeData.rid))
                    {
                        baseBounds = ugcNodeBounds[nodeData.rid];
                    }
                    else
                    {
                        if (curMapData.resList.TryGetValue(nodeData.rid, out var prims))
                        {
                            foreach (var tmpPrim in prims)
                            {
                                var tmpMapNode = new MapNodeData()
                                {
                                    nodeData = tmpPrim,
                                    hash = tmpPrim.ToHash(),
                                    Parent = this,
                                    Children = new List<MapNodeData>()
                                };
                                Children.Add(tmpMapNode);
                                tmpMapNode.Setup();
                            }
                        }
                    }
                }
                else
                {
                    if (nodeBounds.ContainsKey(nodeData.id))
                    {
                        baseBounds = nodeBounds[nodeData.id];
                    }
                }
            }

            if (baseBounds == null)
            {
                Bounds? targetBounds = null;
                foreach (var tmpChild in Children)
                {
                    if (targetBounds == null)
                    {
                        targetBounds = tmpChild.BaseBounds();
                    }
                    else
                    {
                        var tmpBounds = tmpChild.BaseBounds();
                        if (tmpBounds == null) continue;
                        var tmp = targetBounds.Value;
                        tmp.Encapsulate(tmpBounds.Value);
                        targetBounds = tmp;
                    }
                }
                baseBounds = targetBounds;
            }
            if (baseBounds != null && !string.IsNullOrEmpty(nodeData.rid) && !ugcNodeBounds.ContainsKey(nodeData.rid))
            {
                ugcNodeBounds.Add(nodeData.rid, baseBounds);
            }
        }
        return baseBounds;
    }

    private void ResetBounds()
    {
        baseBounds = null;
        bounds = null;
    }

    private Bounds TransformParentBounds(Bounds tmpBounds)
    {
        if (Parent != null)
        {
            var tmpPos = DataUtils.DeSerializeVector3(Parent.nodeData.p);
            var tmpRot = DataUtils.DeSerializeVector3(Parent.nodeData.r);
            var tmpScale = DataUtils.DeSerializeVector3(Parent.nodeData.s);
            tmpBounds = tmpBounds.TransformBounds(Matrix4x4.TRS(tmpPos, Quaternion.Euler(tmpRot), tmpScale));
            Parent.TransformParentBounds(tmpBounds);
            return tmpBounds;
        }
        else
        {
            return tmpBounds;
        }
    }


    public static void InitBounds()
    {
        nodeBounds.Clear();
        foreach (var keyValue in GameManager.Inst.priConfigData)
        {
            nodeBounds.Add(keyValue.Key, keyValue.Value.bounds);
        }
    }

    public static void SetupMap(MapData mapData)
    {
        curMapData = mapData;
        InitBounds();
        foreach (var nodeData in mapData.pref)
        {
            var tmpMapNode = new MapNodeData()
            {
                nodeData = nodeData,
                hash = nodeData.ToHash(),
                Parent = null,
                Children = new List<MapNodeData>()
            };
            tmpMapNode.Setup();
        }
    }

    public static MapNodeData Get(string hash)
    {
        mapNodeDataDic.TryGetValue(hash, out var tmpMapNodeData);
        return tmpMapNodeData;
    }

    public static void Clear()
    {
        curMapData = null;
        nodeBounds.Clear();
        mapNodeDataDic.Clear();
        ugcNodeBounds.Clear();
    }

}
