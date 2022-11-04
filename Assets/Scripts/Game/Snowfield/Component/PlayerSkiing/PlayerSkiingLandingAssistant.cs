#if false
using UnityEngine;

namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private const int PRE_STEP_NUM = 2;
        private const float MAX_PRE_STEP_TIME = 0.25f;
        private const float ROT_CORRECTION_SPEED = 1.5f;

        private Vector3 _preNormal = Vector3.zero;

        private void OnUpdate_LandingAssistant()
        {
            if (State == SkiingState.Air)
            {
                if (PreSearchLandingNormal())
                {
                    float rotAngle = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(_bordUpVector, _preNormal));
                    Vector3 rotAxis = Vector3.Cross(_bordUpVector, _preNormal).normalized;
                    ApplyRotationAxis(rotAxis, rotAngle * ROT_CORRECTION_SPEED);
                }
                else
                {
                    UpdateBordDirection(Vector3.up);
                }
            }
        }

        private bool PreSearchLandingNormal()
        {
            Vector3 currPos = _playerTransform.GetTransform().position;
            Vector3 currVelocity = Velocity;
            for (int i = 0; i < PRE_STEP_NUM; i++)
            {
                if (RayCastForLanding(currPos, currVelocity))
                {
                    return true;
                }
                else
                {
                    currPos += currVelocity * MAX_PRE_STEP_TIME;
                    currVelocity += GetVChangeDueToGravity(MAX_PRE_STEP_TIME);
                }
            }

            return false;
        }

        private bool RayCastForLanding(Vector3 prePos, Vector3 preVelocity)
        {
            var maxDistance = preVelocity.magnitude * MAX_PRE_STEP_TIME;

            RaycastHit hitInfo;
            var isSuccess = Physics.Raycast(prePos, preVelocity.normalized, out hitInfo, maxDistance);
            if (isSuccess)
            {
                _preNormal = hitInfo.normal;
            }

            return isSuccess;
        }
    }
}
#endif