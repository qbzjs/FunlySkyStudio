#if false
using UnityEngine;

namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private float ACCELERATION_FORCE = 180f;
        private float JUMP_UP_SPEED = 10f;
        private float DRAG_AIR = 0.04f;
        private float DRAG_GROUND_MAX = 0.0475f;
        private float ADDITIONAL_GRAVITY_IN_AIR = -5f;

        private void OnFixedUpdate_Physics()
        {
            switch (State)
            {
                case SkiingState.Ground:
                    float dragFadeFactor = 0.5f;
                    if (_joystickOffset.magnitude == 0)
                        dragFadeFactor = 10;

                    float drag = dragFadeFactor * DRAG_GROUND_MAX;
                    _rigidbody.drag = drag;
                    _damperRigidbody.drag = drag;

                    if (_bordUpVector.y > 0.0f)
                    {
                        Vector3 dir = _bordDirection;
                        dir *= ACCELERATION_FORCE * _joystickOffset.magnitude;
                        _rigidbody.AddForce(dir);
                        _damperRigidbody.AddForce(dir);
                    }
                    break;
                case SkiingState.Air:
                    _rigidbody.drag = DRAG_AIR;
                    _damperRigidbody.drag = DRAG_AIR;

                    _rigidbody.AddForce(0f, ADDITIONAL_GRAVITY_IN_AIR, 0f, ForceMode.Acceleration);
                    _damperRigidbody.AddForce(0f, ADDITIONAL_GRAVITY_IN_AIR, 0f, ForceMode.Acceleration);
                    break;
            }
        }

        private Vector3 GetVChangeDueToGravity(float time)
        {
            return Physics.gravity * time;
        }
    }
}
#endif