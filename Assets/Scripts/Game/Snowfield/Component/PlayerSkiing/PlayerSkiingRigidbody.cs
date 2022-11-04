#if false
using UnityEngine;

namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private Rigidbody _rigidbody;
        private SphereCollider _collider;
        private SpringJoint _damperJoint;

        public Vector3 Velocity
        {
            get { return _rigidbody.velocity; }
            set { _rigidbody.velocity = value; }
        }

        private void InitRigidbody()
        {
            _collider = _playerTransform.AddComponent<SphereCollider>();
            _collider.radius = 0.3f;
            _collider.center = new Vector3(0f, 0.25f, 0f);

            var physicMaterial = new PhysicMaterial();
            physicMaterial.dynamicFriction = 0;
            physicMaterial.staticFriction = 0;
            physicMaterial.bounciness = 0;
            physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
            physicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
            _collider.material = physicMaterial;

            _damperJoint = _playerTransform.AddComponent<SpringJoint>();
            _damperJoint.connectedBody = _damperRigidbody;
            _damperJoint.spring = 100000;
            _damperJoint.damper = 4000;
            _damperJoint.autoConfigureConnectedAnchor = false;
            _damperJoint.connectedAnchor = Vector3.zero;

            _rigidbody = _playerTransform.GetComponent<Rigidbody>();
            _rigidbody.mass = 20f;
            _rigidbody.useGravity = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            Physics.IgnoreCollision(_collider, _damperCollider);
        }

        private void DestroyRigidbody()
        {
            GameObject.Destroy(_damperJoint);
            GameObject.Destroy(_rigidbody);
            GameObject.Destroy(_collider);
        }
    }
}
#endif