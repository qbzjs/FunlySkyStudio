#if false
using UnityEngine;

namespace Snowfield
{
    public class PlayerFogFollower : Component, IFrameUpdate
    {
        private const string FOG_PATH = "Prefabs/Model/Snowfield/Fog";

        private PlayerTransform _playerTransform;
        private GameObject _fog;

        public override void Init(Core core)
        {
            base.Init(core);

            _playerTransform = GetCom<PlayerTransform>();

            var prefab = ResManager.Inst.LoadRes<GameObject>(FOG_PATH);
            _fog = GameObject.Instantiate(prefab, SceneBuilder.Inst.SceneParent);
            _fog.Normalize();
        }

        public override void Close()
        {
            base.Close();

            GameObject.Destroy(_fog);
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
            var pos = _playerTransform.GetPosition();
            _fog.transform.localPosition = pos;
        }
    }
}
#endif