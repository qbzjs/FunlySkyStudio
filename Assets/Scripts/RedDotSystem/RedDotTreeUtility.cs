using AvatarRedDotSystem;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using UnityEngine;

namespace RedDot
{
    public enum ERedDotPrefabType
    {
        Type1,//大红点 24*24
        Type2,//大紫点 24*24
        Type3,//小紫点 15*15
        Type4,//小红点 18*18
    }
    public enum ERedDotPos
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
    public static class RedDotTreeUtility
    {
        public const string mRedDotName = "redDot";
        public const string mRedDotPath = "Prefabs/UI/RedDot";
        public static Dictionary<ERedDotPrefabType, string> mRedDotPrefabPaths = new Dictionary<ERedDotPrefabType, string>() {
            {ERedDotPrefabType.Type1,$"{mRedDotPath}/RedDotItem1" },
            {ERedDotPrefabType.Type2,$"{mRedDotPath}/RedDotItem2" },
            {ERedDotPrefabType.Type3,$"{mRedDotPath}/RedDotItem3" },
            {ERedDotPrefabType.Type4,$"{mRedDotPath}/RedDotItem4" },
        };
        //创建和绑定已有的逻辑节点
        public static VNode CreateAndBindViewRedDot(this RedDotTree tree, GameObject target, int logicNodeType, ERedDotPrefabType prefabType, ERedDotPos dotPos = ERedDotPos.TopRight)
        {
            Node logicNode = tree.GetNode(logicNodeType);
            if (logicNode == null)
            {
                UnityEngine.Debug.LogError($"logicNodeType is null ={logicNodeType}");
                return null;
            }
            GameObject objDot = CreateRedDotGameObject(target, prefabType, dotPos);

            VNode vNode = CraeteVNode(objDot, logicNode);
            return vNode;
        }
        /// <summary>
        /// 红点树的扩展
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="target">需要挂红点的GameObject</param>
        /// <param name="parentType">红点的数据父节点</param>
        /// <param name="nodeType">红点自身的类型</param>
        /// <param name="prefabType">红点GameObject的预制件</param>
        /// <param name="dotPos">红点的显示位置，一共有9个位置</param>
        /// <returns></returns>
        public static VNode AddRedDot(this RedDotTree tree, GameObject target, int parentType, int nodeType, ERedDotPrefabType prefabType, ERedDotPos dotPos = ERedDotPos.TopRight)
        {
            Node logicNode = tree.AddNode(parentType, nodeType);
            if (logicNode == null)
            {
                UnityEngine.Debug.LogError($"child is null  {target} parentType= {parentType} nodeType= {nodeType}");
                return null;
            }
            GameObject objDot = CreateRedDotGameObject(target, prefabType, dotPos);

            VNode vNode = CraeteVNode(objDot, logicNode);
            return vNode;
        }
        public static GameObject CreateRedDotGameObject(GameObject target, ERedDotPrefabType prefabType, ERedDotPos dotPos = ERedDotPos.TopRight)
        {
            Transform dot = null;
            GameObject objDot = null;

            string prefabPath = string.Empty;
            if (mRedDotPrefabPaths.TryGetValue(prefabType, out prefabPath))
            {
                GameObject obj = ResManager.Inst.LoadRes<GameObject>(prefabPath);
                objDot = GameObject.Instantiate(obj);
            }
            else
            {
                UnityEngine.Debug.LogError($"prefab type is valid ={prefabType}");
                return null;
            }

            dot = objDot.transform;
            dot.gameObject.name = mRedDotName;

            dot.SetParent(target.transform, false);
            dot.SetAsLastSibling();

            SetRectTransform(dot as RectTransform, dotPos);
            return objDot;
        }
        public static VNode CraeteVNode(GameObject obj, Node logicNode)
        {
            VNode vNode = obj.AddComponent<VNode>();
            vNode.mLogic = logicNode;
            vNode.Init();

            return vNode;
        }

        //仅删除表现层
        public static void OnlyRemoveViewRedDot(VNode vNode)
        {
            vNode.Destroy(false);
        }
        //删除表现层和逻辑层
        public static void RemoveRedDot(VNode vNode)
        {
            vNode.Destroy(true);

        }
        public static void SetRectTransform(RectTransform rectTransForm, ERedDotPos dotPos)
        {
            Vector2 anchorMin = new Vector2();
            Vector2 anchorMax = new Vector2();
            Vector2 pivot = new Vector2();

            switch (dotPos)
            {
                case ERedDotPos.TopLeft:
                    anchorMin.x = 0;
                    anchorMin.y = 1;
                    anchorMax.x = 0;
                    anchorMax.y = 1;
                    pivot.x = 0;
                    pivot.y = 1;
                    break;
                case ERedDotPos.TopCenter:
                    anchorMin.x = 0.5f;
                    anchorMin.y = 1;
                    anchorMax.x = 0.5f;
                    anchorMax.y = 1;
                    pivot.x = 0.5f;
                    pivot.y = 1;
                    break;
                case ERedDotPos.TopRight:
                    anchorMin.x = 1;
                    anchorMin.y = 1;
                    anchorMax.x = 1;
                    anchorMax.y = 1;
                    pivot.x = 1;
                    pivot.y = 1;
                    break;
                case ERedDotPos.MiddleLeft:
                    anchorMin.x = 0;
                    anchorMin.y = 0.5f;
                    anchorMax.x = 0;
                    anchorMax.y = 0.5f;
                    pivot.x = 0;
                    pivot.y = 0.5f;
                    break;
                case ERedDotPos.MiddleCenter:
                    anchorMin.x = 0.5f;
                    anchorMin.y = 0.5f;
                    anchorMax.x = 0.5f;
                    anchorMax.y = 0.5f;
                    pivot.x = 0.5f;
                    pivot.y = 0.5f;
                    break;
                case ERedDotPos.MiddleRight:
                    anchorMin.x = 1;
                    anchorMin.y = 0.5f;
                    anchorMax.x = 1;
                    anchorMax.y = 0.5f;
                    pivot.x = 1;
                    pivot.y = 0.5f;
                    break;
                case ERedDotPos.BottomLeft:
                    anchorMin.x = 0;
                    anchorMin.y = 0;
                    anchorMax.x = 0;
                    anchorMax.y = 0;
                    pivot.x = 0;
                    pivot.y = 0;
                    break;
                case ERedDotPos.BottomCenter:
                    anchorMin.x = 0.5f;
                    anchorMin.y = 0;
                    anchorMax.x = 0.5f;
                    anchorMax.y = 0;
                    pivot.x = 0.5f;
                    pivot.y = 0;
                    break;
                case ERedDotPos.BottomRight:
                    anchorMin.x = 1;
                    anchorMin.y = 0;
                    anchorMax.x = 1;
                    anchorMax.y = 0;
                    pivot.x = 1;
                    pivot.y = 0;
                    break;
            }

            rectTransForm.pivot = pivot;
            rectTransForm.anchorMin = anchorMin;
            rectTransForm.anchorMax = anchorMax;
            rectTransForm.anchoredPosition = Vector2.zero;

        }
    }
}


