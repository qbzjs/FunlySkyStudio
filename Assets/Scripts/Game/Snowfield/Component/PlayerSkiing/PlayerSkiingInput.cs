#if false
using UnityEngine;

namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private float ROT_SPEED = 200f;
        private const float CTRL_SMOOTH_TIME = 5f;

        private float _controlSmootherAxisX = 0.0f;
        private Vector2 _joystickOffset = Vector2.zero;
        private Vector2 _joystickDragOffset = Vector2.zero;
        private int _dir;

        private float AxisX { get { return _joystickDragOffset.x; } }
        private float AxisY { get { return _joystickDragOffset.y; } }

        private void InitInput()
        {
            MessageHelper.AddListener<Vector3>(MessageName.OnPlayerMove, HandleJoystickDrag);
            MessageHelper.AddListener(MessageName.OnPlayerJump, HandleJump);
        }

        private void DestroyInput()
        {
            MessageHelper.RemoveListener<Vector3>(MessageName.OnPlayerMove, HandleJoystickDrag);
            MessageHelper.RemoveListener(MessageName.OnPlayerJump, HandleJump);
        }

        private void HandleJoystickDrag(Vector3 offset)
        {
            if (offset.magnitude == 0)
            {
                _dir = 0;
                _joystickDragOffset = _joystickOffset = Vector2.zero;
                return;
            }

            var joystickNormal = offset.normalized;
            var joyjoystickAngle = Vector3.Angle(joystickNormal, Vector3.up);
            if (joystickNormal.x < 0)
                joyjoystickAngle = 360 - joyjoystickAngle;

            var parentAngle = _playerTransform.GetTransform().localEulerAngles.y;
            var worldAngle = parentAngle + joyjoystickAngle;
            float r = worldAngle * Mathf.Deg2Rad;
            float x = Mathf.Sin(r);
            float y = Mathf.Cos(r);
            offset.x = x;
            offset.y = y;

            _joystickOffset = offset;

            var joysticeDir = new Vector3(offset.normalized.x, 0, offset.normalized.y);
            var bordDir = _bordDirection.normalized;

            float angle = Vector3.Angle(joysticeDir, bordDir);
            var dir = Vector3.Dot(Vector3.up, Vector3.Cross(bordDir, joysticeDir));
            if (Mathf.Abs(dir) < 0.1f)
                _dir = 0;
            else
                _dir = dir < 0 ? -1 : 1;

            angle *= dir < 0 ? -1 : 1;
            _joystickDragOffset = new Vector2(angle / 180f, offset.normalized.y);
        }

        private void HandleJump()
        {
            if (State != SkiingState.Ground)
                return;

            _rigidbody.velocity += new Vector3(0.0f, JUMP_UP_SPEED, 0.0f);
            GoToTakeOff();
        }

        private void OnUpdate_Input()
        {
            switch (State)
            {
                case SkiingState.Ground:
                    HandleGroundRot();
                    break;
            }
        }

        private void HandleGroundRot()
        {
            SmoothControls();
            float rotFactor = _controlSmootherAxisX;
            rotFactor *= ROT_SPEED;
            ApplyRotationYAxis(rotFactor);
        }

        private void SmoothControls()
        {
            if (AxisX != 0f)
                _controlSmootherAxisX = Mathf.Lerp(_controlSmootherAxisX, AxisX, Time.deltaTime * CTRL_SMOOTH_TIME);
            else
                _controlSmootherAxisX = 0f;
        }
    }
}
#endif