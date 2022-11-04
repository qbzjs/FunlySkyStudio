#if false
using UnityEngine;
using System;

namespace Snowfield
{
    public class Damper : MonoBehaviour
    {
        public Action<Collision> onCollisionEnter;
        public Action<Collision> onCollisionStay;
        public Action<Collision> onCollisionExit;

        private void OnCollisionEnter(Collision collision)
        {
            onCollisionEnter?.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            onCollisionStay?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            onCollisionExit?.Invoke(collision);
        }
    }
}
#endif