#if false
using UnityEngine;

namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private const string SKATEBOARD_PATH = "Prefabs/Model/Special/SkateBoard";
        private const string SNOWEFFECT_PATH = "Effect/SnowSkate/skateboardEffect";

        private GameObject _skateboard;
        private Animator _boardAnimation;

        private Vector3 _bordDirection;
        private Vector3 _bordUpVector;

        private void InitBoard()
        {
            var prefab = ResManager.Inst.LoadRes<GameObject>(SKATEBOARD_PATH);
            _skateboard = GameObject.Instantiate(prefab, _model);
            _skateboard.Normalize();

            prefab = ResManager.Inst.LoadRes<GameObject>(SNOWEFFECT_PATH);
            var effect = GameObject.Instantiate(prefab, _skateboard.transform);
            effect.Normalize();

            _boardAnimation = _skateboard.GetComponent<Animator>();

            _bordDirection = _model.forward;
            _bordUpVector = _model.up;
        }

        private void DestroyBoard()
        {
            GameObject.Destroy(_skateboard);
        }

        private void UpdateBordDirection(Vector3 bordUpVector)
        {
            _bordUpVector = bordUpVector;

            Vector3 lateralDirection = Vector3.Cross(_bordDirection, _bordUpVector);
            _bordDirection = Vector3.Cross(_bordUpVector, lateralDirection).normalized;
        }

        private void ApplyRotationAxis(Vector3 pAxis, float rotFactor)
        {
            if (rotFactor != 0.0f && !System.Single.IsNaN(rotFactor * Time.deltaTime))
            {
                Quaternion rotQuat = Quaternion.AngleAxis(rotFactor * Time.deltaTime, pAxis);
                _bordDirection = rotQuat * _bordDirection;
                _bordUpVector = rotQuat * _bordUpVector;
            }
        }

        private void ApplyRotationYAxis(float rotFactor)
        {
            if (rotFactor != 0.0f && !System.Single.IsNaN(rotFactor * Time.deltaTime))
            {
                Quaternion rotQuat = Quaternion.AngleAxis(rotFactor * Time.deltaTime, _bordUpVector);
                _bordDirection = rotQuat * _bordDirection;
            }
        }
    }
}
#endif