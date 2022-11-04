using UnityEngine;

public class VIPCheckBoundControl : MonoBehaviour
{
    public GameObject boundEffect;
    public Vector3 center;
    public Vector3 size;

    public void InitEffect(bool tryFindInParent = true)
    {
        var parent = transform.parent;
        if (parent != null && tryFindInParent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name.Contains("VIPCheckEffect"))
                {
                    boundEffect = child.gameObject;
                    break;
                }
            }
        }
        if (boundEffect == null)
        {
            GameObject prefab = ResManager.Inst.LoadRes<GameObject>("Prefabs/Model/Special/VIPCheckEffect");
            boundEffect = Instantiate(prefab, transform.parent);
            boundEffect.transform.localPosition = Vector3.zero;
        }
    }
    
    public void UpdateEffectShow()
    {
        if (boundEffect == null)
        {
            return;
        }
        Renderer[] renders = GetComponentsInChildren<Renderer>();
        if(renders.Length == 0)
        {
            LoggerUtils.Log("UpdateBoundBox renders.Length = " + renders.Length);
            return;
        }
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;
        foreach (Renderer child in renders)
        {
            minX = Mathf.Min(minX, child.bounds.min.x);
            maxX = Mathf.Max(maxX, child.bounds.max.x);
            minY = Mathf.Min(minY, child.bounds.min.y);
            maxY = Mathf.Max(maxY, child.bounds.max.y);
            minZ = Mathf.Min(minZ, child.bounds.min.z);
            maxZ = Mathf.Max(maxZ, child.bounds.max.z);
        }
        center = new Vector3((minX+maxX)/2,(minY+maxY)/2,(minZ+maxZ)/2);
        size = new Vector3(maxX-minX,maxY-minY,maxZ-minZ);
        float max = Mathf.Max(size.x, size.z);
        boundEffect.transform.SetParent(transform.parent);
        boundEffect.transform.position = new Vector3(center.x,minY,center.z);
        boundEffect.transform.localScale = Vector3.one * max / boundEffect.transform.parent.localScale.x;
        //整体缩小一点
        boundEffect.transform.localScale *= VIPZoneConstant.FACTOR_CHECK_EFFECT;
    }

    public void SetTriggerActive(bool active)
    {
        VIPZoneBehaviour vipZoneBehaviour = GetComponentInParent<VIPZoneBehaviour>();
        if (active)
        {
            //更新一下
            UpdateEffectShow();
            vipZoneBehaviour.EnableCollider(center,size);
        }
        else
        {
            vipZoneBehaviour.DisableCollider();
        }
    }

    public void OnReset()
    {
        if (boundEffect != null)
        {
            Destroy(boundEffect.gameObject);
        }
    }

    void OnDestroy()
    {
        if (boundEffect != null)
        {
            Destroy(boundEffect.gameObject);
        }
    }
}