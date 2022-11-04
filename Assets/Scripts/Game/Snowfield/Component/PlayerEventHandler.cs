#if false
namespace Snowfield
{
    public class PlayerEventHandler : Component
    {
        public override void Init(Core core)
        {
            base.Init(core);

            MessageHelper.AddListener(MessageName.OnEnterSnowfield, HandleEnterSnowfield);
            MessageHelper.AddListener(MessageName.OnLeaveSnowfield, HandleLeaveSnowfield);
        }

        public override void Close()
        {
            base.Close();

            MessageHelper.RemoveListener(MessageName.OnEnterSnowfield, HandleEnterSnowfield);
            MessageHelper.RemoveListener(MessageName.OnLeaveSnowfield, HandleLeaveSnowfield);
        }

        private void HandleEnterSnowfield()
        {
            _core.Add("PlayerFogFollower", new PlayerFogFollower());
            _core.Add("PlayerRaycaster", new PlayerRaycaster());
            _core.Add("PlayerStateMachine", new PlayerStateMachine());
        }

        private void HandleLeaveSnowfield()
        {
            _core.Remove("PlayerStateMachine");
            _core.Remove("PlayerRaycaster");
            _core.Remove("PlayerFogFollower");
        }
    }
}
#endif