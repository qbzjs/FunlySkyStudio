#if false
using UnityEngine;

namespace Snowfield
{
    public class PlayerFrameHander : Component
    {
        private const string SKATEBOARD_PATH = "Prefabs/Model/Special/SkateBoard";
        private const string SNOWEFFECT_PATH = "Effect/SnowSkate/skateboardEffect";

        private PlayerTransform _playerTransform;
        private GameObject _skateboard;
        private int _state = State.Default;

        public override void Init(Core core)
        {
            base.Init(core);

            _playerTransform = GetCom<PlayerTransform>();
        }

        public override void Close()
        {
            base.Close();
        }

        public void Sync(int state)
        {
            if (_state == state)
                return;

            _state = state;

            if (state == State.Skiing)
                InitBoard();
            else if (state != State.StopSkiing)
                DestroyBoard();
        }

        private void InitBoard()
        {
            var prefab = ResManager.Inst.LoadRes<GameObject>(SKATEBOARD_PATH);
            _skateboard = GameObject.Instantiate(prefab, _playerTransform.GetTransform());
            _skateboard.Normalize();

            prefab = ResManager.Inst.LoadRes<GameObject>(SNOWEFFECT_PATH);
            var effect = GameObject.Instantiate(prefab, _skateboard.transform);
            effect.Normalize();
        }

        private void DestroyBoard()
        {
            GameObject.Destroy(_skateboard);
        }
    }
}
#endif