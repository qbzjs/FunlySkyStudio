#if false
using UnityEngine;

namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private const string OLD_ANIMATOR_PATH = "Animation/PlayerAnim";
        private const string ANIMATOR_PATH = "Animation/PlayerAnimation";

        private Transform _model;

        private const float DAMPER_Y_OFFSET = -0.1f;
        private const float MODEL_EULER_ROT_SPEED = 12.0f;
        private const float MODEL_UP_VECT_SPEED = 12.0f;

        private float _currEuler = 0.0f;
        private Vector3 _currDirection = Vector3.forward;
        private Vector3 _currUpVector = Vector3.up;

        private void InitModel()
        {
            _model = _playerTransform.GetTransform().Find("Player");

            _playerAnimation.SetAnimator(ResManager.Inst.LoadRes<RuntimeAnimatorController>(ANIMATOR_PATH));

            var characterController = _playerTransform.GetComponent<CharacterController>();
            if (characterController != null)
                characterController.enabled = false;
        }

        private void DestroyModel()
        {
            _playerAnimation.SetAnimator(ResManager.Inst.LoadRes<RuntimeAnimatorController>(OLD_ANIMATOR_PATH));

            var characterController = _playerTransform.GetComponent<CharacterController>();
            if (characterController != null)
                characterController.enabled = true;
        }

        public void OnUpdate_Model()
        {
            _currUpVector = Vector3.Slerp(_currUpVector, _bordUpVector, Mathf.Clamp01(MODEL_UP_VECT_SPEED * Time.deltaTime));
            _currDirection = Vector3.Slerp(_currDirection, _bordDirection, Mathf.Clamp01(MODEL_UP_VECT_SPEED * Time.deltaTime));

            Vector3 euler = Vector3.zero;
            float eulerValue = 0.0f;

            _currEuler = Mathf.Lerp(_currEuler, eulerValue, Mathf.Clamp01(MODEL_EULER_ROT_SPEED * Time.deltaTime));
            euler.x += _currEuler;

            _model.transform.rotation = Quaternion.LookRotation(_currDirection, _currUpVector);
            _model.transform.rotation *= Quaternion.Euler(euler);

            _damperJoint.anchor = _playerTransform.GetTransform().InverseTransformPoint(_model.transform.position + _model.up * (_damperCollider.radius + DAMPER_Y_OFFSET));
        }
    }
}
#endif