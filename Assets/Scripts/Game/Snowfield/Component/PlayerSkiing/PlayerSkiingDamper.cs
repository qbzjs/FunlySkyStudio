#if false
using UnityEngine;
using System.Collections.Generic;

namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private Transform _damper;
        private SphereCollider _damperCollider;
        private Rigidbody _damperRigidbody;
        private List<Collision> _collisions;

        private bool _keepSameUpVector = false;
        private Vector3 _bordUpVectorAccumulator = Vector3.zero;
        private Vector3 _lastCollisionPointAccumulator = Vector3.zero;
        private int _lastCollisionPointAccumulatorNum = 0;
        private Vector3 _lastCollisionPoint = Vector3.zero;

        private void InitDamper()
        {
            _collisions = new List<Collision>();

            _damper = new GameObject("Damper").transform;
            _damper.transform.parent = _playerTransform.GetTransform();
            _damper.Normalize();

            _damperCollider = _damper.gameObject.AddComponent<SphereCollider>();
            _damperCollider.radius = 0.5f;
            _damperCollider.center = Vector3.zero;

            _damperRigidbody = _damper.gameObject.AddComponent<Rigidbody>();
            _damperRigidbody.mass = 8f;
            _damperRigidbody.drag = 0f;
            _damperRigidbody.angularDrag = 0.05f;
            _damperRigidbody.useGravity = true;
            _damperRigidbody.interpolation = RigidbodyInterpolation.None;
            _damperRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _damperRigidbody.constraints = RigidbodyConstraints.None;

            var damperScript = _damper.gameObject.AddComponent<Damper>();
            damperScript.onCollisionEnter += HandleCollisionEnter;
            damperScript.onCollisionStay += HandleCollisionStay;
        }

        private void DestroyDamper()
        {
            GameObject.Destroy(_damper.gameObject);
        }

        private void OnUpdate_Damper()
        {
            ApplyCollision();
        }

        private void OnFixedUpdate_Damper()
        {
            foreach (Collision collision in _collisions)
            {
                if (collision != null &&
                    collision.collider != null &&
                    collision.gameObject != null)
                {
                    SaveCollision(collision);
                    SetIsAir(false);
                }
            }

            _collisions.Clear();
            _damperRigidbody.velocity = Velocity * 0.25f + _damperRigidbody.velocity * 0.75f;
        }

        private void SaveCollision(Collision collision)
        {
            if (collision.contacts.Length > 0)
            {
                foreach (ContactPoint contP in collision.contacts)
                {
                    if (! _keepSameUpVector)
                    {
                        _bordUpVectorAccumulator += contP.normal;
                    }

                    _lastCollisionPointAccumulator += contP.point;
                    _lastCollisionPointAccumulatorNum++;

                    if (_state == SkiingState.Ground)
                    {
                        if ((contP.normal - _bordUpVector).magnitude < 0.3f)
                        {
                            _bordUpVectorAccumulator = contP.normal;
                            _keepSameUpVector = true;
                        }
                    }
                }
            }
        }

        private void ApplyCollision()
        {
            if (_lastCollisionPointAccumulatorNum > 0)
            {
                _lastCollisionPoint = _lastCollisionPointAccumulator / (float)_lastCollisionPointAccumulatorNum;
                _lastCollisionPointAccumulator = Vector3.zero;
                _lastCollisionPointAccumulatorNum = 0;

                _bordUpVectorAccumulator.Normalize();
                RaycastHit hit;
                Vector3 damperToCollision = _lastCollisionPoint - _damper.position;
                if (Physics.Raycast(_damper.position, damperToCollision, out hit, damperToCollision.magnitude * 1.35f))
                {
                    if ((_bordUpVectorAccumulator - hit.normal).magnitude < 0.3f)
                        UpdateBordDirection(hit.normal);
                    else
                        UpdateBordDirection(_bordUpVectorAccumulator);
                }

                _bordUpVectorAccumulator = Vector3.zero;
            }

            _keepSameUpVector = false;
        }

        private void HandleCollisionEnter(Collision collision)
        {
            _collisions.Add(collision);
        }

        private void HandleCollisionStay(Collision collision)
        {
            _collisions.Add(collision);
        }
    }
}
#endif