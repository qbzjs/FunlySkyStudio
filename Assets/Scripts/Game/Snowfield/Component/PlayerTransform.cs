#if false
using UnityEngine;

namespace Snowfield
{
    public class PlayerTransform : Component
    {
        private Transform _transform;

        public PlayerTransform(GameObject model)
        {
            _transform = model.transform;
        }

        public override void Init(Core core)
        {
            base.Init(core);
        }

        public override void Close()
        {
            base.Close();

            _transform = null;
        }

        public Transform GetTransform()
        {
            return _transform;
        }

        public T GetComponent<T>() where T : UnityEngine.Component
        {
            return _transform.gameObject.GetComponent<T>();
        }

        public T AddComponent<T>() where T : UnityEngine.Component
        {
            var com = GetComponent<T>();
            if (com != null)
                return com;

            return _transform.gameObject.AddComponent<T>();
        }

        public void RemoveComponent<T>() where T : UnityEngine.Component
        {
            var com = GetComponent<T>();
            if (com == null)
                return;

            GameObject.Destroy(com);
        }

        public Vector3 GetPosition()
        {
            return _transform.localPosition;
        }

        public void SetPosition(Vector3 pos)
        {
            if (_transform.localPosition == pos)
                return;

            _transform.localPosition = pos;
        }

        public Quaternion GetRotation()
        {
            return _transform.localRotation;
        }

        public void SetRotation(Quaternion rotation)
        {
            if (_transform.localRotation == rotation)
                return;

            _transform.localRotation = rotation;
        }

        public Vector3 GetEulerAngles()
        {
            return _transform.localEulerAngles;
        }

        public void SetEulerAngles(Vector3 eulerAngles)
        {
            if (_transform.localEulerAngles == eulerAngles)
                return;

            _transform.localEulerAngles = eulerAngles;
        }

        public Vector3 GetScale()
        {
            return _transform.localScale;
        }

        public void SetScale(Vector3 scale)
        {
            if (_transform.localScale == scale)
                return;

            _transform.localScale = scale;
        }

        public Vector3 GetForward()
        {
            return _transform.forward;
        }

        public Vector3 GetBackword()
        {
            return -_transform.forward;
        }

        public Vector3 GetUp()
        {
            return _transform.up;
        }

        public Vector3 GetDown()
        {
            return -_transform.up;
        }

        public Vector3 GetRight()
        {
            return _transform.right;
        }

        public Vector3 GetLeft()
        {
            return -_transform.right;
        }
    }
}
#endif