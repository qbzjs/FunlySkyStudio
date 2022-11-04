#if false
namespace Snowfield
{
    public partial class PlayerSkiing : Component, IFrameUpdate, IFrameFixedUpdate
    {
        private PlayerTransform _playerTransform;
        private PlayerAnimation _playerAnimation;
        private bool _running;

        public override void Init(Core core)
        {
            base.Init(core);

            _playerTransform = GetCom<PlayerTransform>();
            _playerAnimation = GetCom<PlayerAnimation>();

            InitModel();
            InitBoard();
            InitDamper();
            InitRigidbody();
            InitInput();

            _running = true;
            Start();
        }

        public override void Close()
        {
            base.Close();

            DestroyInput();
            DestroyRigidbody();
            DestroyDamper();
            DestroyBoard();
            DestroyModel();

            _playerTransform = null;
            _playerAnimation = null;
        }

        public void Stop()
        {
            TurnEnd();
            _running = false;
        }

        public override void OnAdded()
        {
            base.OnAdded();

            Init(_core);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();

            Close();
        }

        public void OnUpdate(float deltaTime)
        {
            if (! _running)
                return;

            OnUpdate_Damper();
            OnUpdate_Input();
            OnUpdate_Action();
            OnUpdate_Model();
            OnUpdate_LandingAssistant();
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            if (! _running)
                return;

            OnFixedUpdate_State();
            OnFixedUpdate_Physics();
            OnFixedUpdate_Damper();
        }
    }
}
#endif