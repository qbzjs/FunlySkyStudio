#if false
using UnityEngine;

namespace Snowfield
{
    public class PlayerModel : MonoBehaviour
    {
        private Player _player;

        private void Awake()
        {
            var animator = transform.Find("Player").GetComponent<Animator>();

            _player = new Player(gameObject, animator);
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