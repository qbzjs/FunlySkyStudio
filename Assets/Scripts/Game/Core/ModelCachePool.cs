using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game.Core
{
    public class ModelCachePool : InstMonoBehaviour<ModelCachePool>
    {
        public int maxLength = 10000;
        private int curLength = 0;
        private Dictionary<int, List<GameObject>> noUserPool = new Dictionary<int, List<GameObject>>();


        public GameObject Get(int id)
        {
            GameObject go = null;
            if (noUserPool.ContainsKey(id) && noUserPool[id].Count > 0)
            {
                curLength--;
                go = noUserPool[id].First();
                noUserPool[id].RemoveAt(0);
            }
            else
            {
                go = CreateNode(id);
            }
            go.SetActive(true);
            return go;
        }
        
        public void GetSync(BaseHLODBehaviour nodeBaseBehaviour, int id, Action<GameObject> onSucc = null, Action onFail = null)
        {
            if (nodeBaseBehaviour.assetObj != null)
            {
                onFail?.Invoke();
                return;
            }
            
            if (noUserPool.ContainsKey(id) && noUserPool[id].Count > 0)
            {
                curLength--;
                var go = noUserPool[id].First();
                noUserPool[id].RemoveAt(0);
                go.SetActive(true);
                onSucc?.Invoke(go);
            }
            else
            {
                CreateNodeGetSync(nodeBaseBehaviour, id, onSucc, onFail);
            }
        }
        
        private GameObject CreateNode(int id)
        {
            var resType = GameConsts.GetResType(id);
            if (resType == GameResType.CombEmpty || resType == GameResType.UGCComb)
            {
                return new GameObject("cNode");
            }

            GameObject assetPrefab = null;
            var modelData = GameManager.Inst.priConfigData[id];
            if (modelData.loadFromBundle)
            {
                var prefabName = Path.GetFileName(modelData.prefabName);
                var bundle = BundleMgr.Inst.LoadBundle(BundlePart.Respgc, prefabName);
                assetPrefab = bundle.LoadAsset<GameObject>(prefabName);
                RefreshMaterial(assetPrefab.transform);
            }
            else
            {
                assetPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.BaseModelPath + modelData.prefabName);//实例化预制体
            }
            
            var go = GameObject.Instantiate(assetPrefab);
            go.name = assetPrefab.name;
            return go;
        }

        private void CreateNodeGetSync(BaseHLODBehaviour nodeBaseBehaviour, int id, Action<GameObject> onSucc = null, Action onFail = null)
        {
            var resType = GameConsts.GetResType(id);
            if (resType == GameResType.CombEmpty || resType == GameResType.UGCComb)
            {
                onSucc?.Invoke(new GameObject("cNode"));
                return;
            }
            
            var modelData = GameManager.Inst.priConfigData[id];
            if (modelData.loadFromBundle)
            {
                var prefabName = Path.GetFileName(modelData.prefabName);
                BundleMgr.Inst.LoadBundle(BundlePart.Respgc, prefabName, (bundle) =>
                {
                    if (nodeBaseBehaviour == null || nodeBaseBehaviour.assetObj != null)
                    {
                        onFail?.Invoke();
                        return;
                    }
                    var assetPrefab = bundle.LoadAsset<GameObject>(prefabName);
                    RefreshMaterial(assetPrefab.transform);
                    var go = Instantiate(assetPrefab);
                    go.name = assetPrefab.name;
                    onSucc?.Invoke(go);
                }, onFail);
            }
            else
            {
                var assetPrefab = ResManager.Inst.LoadRes<GameObject>(GameConsts.BaseModelPath + modelData.prefabName);//实例化预制体
                var go = Instantiate(assetPrefab);
                go.name = assetPrefab.name;
                onSucc?.Invoke(go);
            }
            
        }
        
        public void Release(int id, GameObject go)
        {
     
            if (curLength >= maxLength)
            {
                GameObject.Destroy(go);
                return;
            }

            // 如果是UGC 资源 不做缓存
            if (id == (int)GameResType.UGCComb)
            {
                GameObject.Destroy(go);
                return;
            }
            
            curLength++;
            go.transform.SetParent(this.transform);
            go.gameObject.SetActive(false);
            go.transform.localScale = Vector3.one;
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (!noUserPool.ContainsKey(id))
            {
                noUserPool.Add(id, new List<GameObject>());
            }
            noUserPool[id].Add(go);
        }

        private static void RefreshMaterial(Transform t)
        {
            var mrs = t.GetComponentsInChildren<Renderer>();
            foreach (var m in mrs)
            {
                var mts = m.sharedMaterials;
                foreach (var mt in mts)
                {
                    if (mt != null && mt.shader != null)
                    {
                        var rq = mt.renderQueue;
                        mt.shader = Shader.Find(mt.shader.name);
                        mt.renderQueue = rq;
                    }
                }
            }
        }
        
        
        private void OnDestroy()
        {
            inst = null;
        }
    }
}
