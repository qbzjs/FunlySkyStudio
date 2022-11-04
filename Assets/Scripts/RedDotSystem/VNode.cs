using UnityEngine;

namespace RedDot
{
    public class VNode : MonoBehaviour
    {
        public Node mLogic;
        public void Init()
        {
            mLogic.AddListener(RedDotChangedCallBack);
            SetVisible(mLogic.Count>0);
        }
        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
        public void RedDotChangedCallBack(int count)
        {
            SetVisible(count > 0);
        }
        public void Destroy(bool isDestoryLogic)
        {
            mLogic.RemoveListener(RedDotChangedCallBack);
            if (isDestoryLogic)
            {
                mLogic.AddToWaittingDestroy();
            }
            mLogic = null;
            GameObject.Destroy(gameObject);
        }
    }
}


