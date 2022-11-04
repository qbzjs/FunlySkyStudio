using UnityEngine;

public class PooledParticleObjScript : MonoBehaviour
{
    public string mPrefabKey;
    public bool mIsInit;
    public bool mInUse;
    public bool mIsObjDestory; 
    public GameObject mGameObject;
    public Transform mTransform;
    public Vector3 mDefaultScale;
    public ParticleSystem[] ParticleSystems
    {
        get
        {
            if (mGameObject == null)
                return null;

            return mGameObject.GetComponents<ParticleSystem>();
        }
    }
    public new int GetInstanceID()
    {
        return mGameObject != null ? mGameObject.GetInstanceID() : 0;
    }
    public void Initialize(string prefabKey)
    {
        mPrefabKey = prefabKey;
        mGameObject = base.gameObject;
        mTransform = base.transform;
        mDefaultScale = this.gameObject.transform.localScale;
        mIsInit = true;
        mInUse = false;
        mIsObjDestory = false;
    }
    //第一次创建时候被调用
    public void OnCreate()
    {

    }
    //从对象池取出
    public void OnGet()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        var ps = ParticleSystems;
        if (ps != null)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                var par = ps[i];
                if (par != null)
                {
                    ParticleSystem.EmissionModule emission = par.emission;
                    emission.enabled = true;
                    par.time = 0;
                    ParticleSystem.MainModule main = par.main;
                    if (ParticleSystemSimulationSpace.World == main.simulationSpace)
                        par.Simulate(0, false, true);
                    par.Play(false);
                }
            }
        }
        mInUse = true;
    }
    public void OnRecycle()
    {
        this.gameObject.SetActive(false);
        var ps = ParticleSystems;
        if (ps != null)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i] != null)
                {
                    ps[i].Clear(false);
                    ps[i].Stop(false); 
                    ParticleSystem.EmissionModule emission = ps[i].emission;
                    emission.enabled = false;
                }
            }
        }
        mInUse = false;
    }
}

