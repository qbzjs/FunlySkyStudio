#if false
namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private enum SkiingState
        {
            Ground,
            TakeOff,
            Air
        }

        private const int COLLISION_ABSENCE_MAX = 20;
        private const int COLLISION_ABSENCE_MAX_DIV_2 = COLLISION_ABSENCE_MAX / 2;

        private SkiingState _state = SkiingState.Air;

        private int m_collisionAbsenceCounter = COLLISION_ABSENCE_MAX;

        private SkiingState State { get { return _state; } }

        private void GoToTakeOff()
        {
            _state = SkiingState.TakeOff;
        }

        private void SetIsAir(bool value)
        {
            m_collisionAbsenceCounter = COLLISION_ABSENCE_MAX;

            switch (_state)
            {
                case SkiingState.Ground:
                case SkiingState.TakeOff:
                    if (value)
                    {
                        _state = SkiingState.Air;
                    }
                    break;
                case SkiingState.Air:
                    if (!value)
                    {
                        _state = SkiingState.Ground;
                    }
                    break;
            }
        }

        public void OnFixedUpdate_State()
        {
            if (_state == SkiingState.TakeOff)
                m_collisionAbsenceCounter -= 2;
            else
                m_collisionAbsenceCounter--;

            if (m_collisionAbsenceCounter < 0 ||
                (m_collisionAbsenceCounter < COLLISION_ABSENCE_MAX_DIV_2))
            {
                SetIsAir(true);
            }
        }
    }
}
#endif