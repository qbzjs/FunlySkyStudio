using UnityEngine;

class TipsGameObjCreater
{
    private BudTimer budTimer;
    private GameObject m_TipsGameObj;
    public GameObject CreateTipsGameObj(Vector3 pos, Transform parent)
    {
        var parentGo = new GameObject("tmpGo");
        parentGo.transform.SetParent(parent);
        parentGo.transform.localPosition = Vector3.zero;
        parentGo.transform.localRotation = Quaternion.identity;
        parentGo.transform.localScale = Vector3.one;

        m_TipsGameObj = ResManager.Inst.LoadRes<GameObject>("Prefabs/Rp/rpPrefLine");
        m_TipsGameObj = UnityEngine.Object.Instantiate(m_TipsGameObj);
        m_TipsGameObj.transform.SetParent(parentGo.transform);
        m_TipsGameObj.transform.localPosition = pos;
        m_TipsGameObj.transform.localRotation = Quaternion.identity;
        return parentGo;
    }

    public void SetTimeOut()
    {
        if (m_TipsGameObj == null) return;
        StopTimer();
        budTimer = TimerManager.Inst.RunOnce("TipsGameObjCreater", 30, 
            ()=>
            {
                if (m_TipsGameObj == null) return;
                TipPanel.ShowToast("Adding prop failed.");
                DestroyTipsGameObj();
            });
    }
    
    private void StopTimer()
    {
        if(budTimer != null)
        {
            TimerManager.Inst.Stop(budTimer);
            budTimer = null;
        }
    }
    
    public void SetTipsParent(NodeBaseBehaviour parentNode,GameObject selfGameObj)
    {
        selfGameObj.transform.SetParent(parentNode.transform);
        if (parentNode is UGCCombBehaviour ugcParentNode && ugcParentNode.modelType == UGCModelType.None) return;
        DestroyTipsGameObj();
    }
    
    public void DestroyTipsGameObj()
    {
        if (m_TipsGameObj != null)
        {
            UnityEngine.Object.DestroyImmediate(m_TipsGameObj);
            m_TipsGameObj = null;
            StopTimer();
        }
            
    }
}
