#if false
using UnityEngine;

namespace Snowfield
{
    public class OtherPlayer : Core
    {
        public OtherPlayer(GameObject model, Animator animator)
        {
            Add("PlayerTransform", new PlayerTransform(model));
            Add("PlayerAnimation", new PlayerAnimation(animator));
            Add("PlayerFrameHander", new PlayerFrameHander());
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            base.FixedUpdate(fixedDeltaTime);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }
    }
}
#endif