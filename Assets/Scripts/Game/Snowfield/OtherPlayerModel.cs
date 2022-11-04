#if false
using UnityEngine;

namespace Snowfield
{
    public class OtherPlayerModel : MonoBehaviour
    {
        private OtherPlayer _player;

        public void SetData(OtherPlayer player)
        {
            _player = player;
        }

        private void Awake()
        {
            _player.Initialize();
        }

        private void OnDestroy()
        {
            _player.Close();
        }

        private void Update()
        {
            _player.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _player.FixedUpdate(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            _player.LateUpdate();
        }
    }
}
#endif