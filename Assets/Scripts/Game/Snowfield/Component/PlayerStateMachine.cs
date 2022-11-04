#if false
using UnityEngine;

namespace Snowfield
{
    public class PlayerStateMachine : Component, IFrameUpdate
    {
        private Player _player;
        private PlayerRaycaster _raycaster;
        private StateMachine _stateMachine;

        public override void Init(Core core)
        {
            base.Init(core);

            _player = Player.Instance;
            _raycaster = GetCom<PlayerRaycaster>();

            _stateMachine = new StateMachine("PlayerStateMachine");
            InitStateMachine();
            _stateMachine.Start();
        }

        public override void Close()
        {
            base.Close();

            _stateMachine.Destroy();
            _player.Remove("PlayerSkiing");

            _stateMachine = null;
            _player = null;
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
            UpdateState();
            _stateMachine.Run();
        }

        private void InitStateMachine()
        {
            _stateMachine.NewState("Normal").Run(() => {
                //Do nothing
            }).AsDefault();

            _stateMachine.NewState("Skiing").Run(() => {
                //Do nothing
            }).OnRunIn(() => {
                _player.Add("PlayerSkiing", new PlayerSkiing());
            });

            _stateMachine.NewState("StopSkiing").Run(() => {
                //Do nothing
            }).OnRunIn(() => {
                _player.Get<PlayerSkiing>().Stop();
                TimerManager.Inst.RunOnce("SkiingTimer", 0.5f, () => {
                    _player.State = State.Default;
                    _player.Remove("PlayerSkiing");
                });
            });

            _stateMachine.Trans().From("Normal").To("Skiing").When((st) => _player.State == State.Skiing);
            _stateMachine.Trans().From("Skiing").To("StopSkiing").When((st) => _player.State != State.Skiing);
            _stateMachine.Trans().From("StopSkiing").To("Normal").When((st) => _player.State != State.StopSkiing);
        }

        private void UpdateState()
        {
            var hitInfos = _raycaster.HitInfos;
            foreach (var hitInfo in hitInfos)
            {
                if (hitInfo.gameObject.layer == LayerMask.NameToLayer("WaterCube"))
                {
                    _player.State = State.Swiming;
                    return;
                }

                if (hitInfo.gameObject.layer == LayerMask.NameToLayer("IceCube"))
                {
                    _player.State = State.Skating;
                    return;
                }
            }

            if (_player.State != State.Skiing && _player.State != State.StopSkiing)
                _player.State = State.Default;
        }
    }
}
#endif