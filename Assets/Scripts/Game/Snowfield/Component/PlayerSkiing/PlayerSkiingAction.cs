#if false
namespace Snowfield
{
    public partial class PlayerSkiing
    {
        private void OnUpdate_Action()
        {
            switch (State)
            {
                case SkiingState.Ground:
                    if (_dir == -1)
                        TurnLeft();
                    else if (_dir == 1)
                        TurnRight();
                    else
                        Idle();
                    break;
                case SkiingState.TakeOff:
                    Jump();
                    break;
            }
        }

        private void Start()
        {
            _playerAnimation.Play(PlayerSnowSkateAnim.skiing_in);
            _boardAnimation.Play(PlayerSnowSkateAnim.skiing_in);
        }

        private void Idle()
        {
            _playerAnimation.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, (int)SnowAnimState.ForWoard);
            _boardAnimation.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, (int)SnowAnimState.ForWoard);
        }

        private void Jump()
        {
            _playerAnimation.Play("skiing_jump");
            _boardAnimation.Play("skiing_jump");
        }

        private void TurnLeft()
        {
            _playerAnimation.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, (int)SnowAnimState.Left);
            _boardAnimation.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, (int)SnowAnimState.Left);
        }

        private void TurnRight()
        {
            _playerAnimation.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, (int)SnowAnimState.Right);
            _boardAnimation.SetInteger(PlayerSnowSkateAnim.SnowAnimPara_RunOffset, (int)SnowAnimState.Right);
        }

        public void TurnEnd()
        {
            _playerAnimation.Play(PlayerSnowSkateAnim.skiing_out);
            _boardAnimation.Play(PlayerSnowSkateAnim.skiing_out);
        }
    }
}
#endif