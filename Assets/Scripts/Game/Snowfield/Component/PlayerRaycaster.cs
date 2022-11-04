#if false
using UnityEngine;

namespace Snowfield
{
    public class PlayerRaycaster : Component, IFrameUpdate
    {
        public Collider[] HitInfos { get { return _hitInfos; } }

        private const float MAX_DISTANCE = 1f;

        private Collider[] _hitInfos;
        private PlayerTransform _playerTransform;

        public override void Init(Core core)
        {
            base.Init(core);

            _playerTransform = GetCom<PlayerTransform>();
        }

        public override void Close()
        {
            base.Close();

            _playerTransform = null;
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
            _hitInfos = Physics.OverlapSphere(_playerTransform.GetPosition(), MAX_DISTANCE);
        }
    }
}
#endif