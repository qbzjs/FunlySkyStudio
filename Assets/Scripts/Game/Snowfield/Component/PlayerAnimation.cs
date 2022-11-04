#if false
using UnityEngine;

namespace Snowfield
{
    public class PlayerAnimation : Component
    {
        private Animator _animator;

        public PlayerAnimation(Animator animator)
        {
            _animator = animator;
        }

        public override void Init(Core core)
        {
            base.Init(core);
        }

        public override void Close()
        {
            base.Close();

            _animator = null;
        }

        public void Play(string name)
        {
            _animator.Play(name);
        }

        public void SetInteger(string name, int value)
        {
            _animator.SetInteger(name, value);
        }

        public void SetAnimator(RuntimeAnimatorController animator)
        {
            _animator.runtimeAnimatorController = animator;
        }
    }
}
#endif