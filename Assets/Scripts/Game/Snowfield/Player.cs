#if false
using UnityEngine;

namespace Snowfield
{
    public class State
    {
        public const int Default = 0;
        public const int Skiing = 1;
        public const int StopSkiing = 2;
        public const int Swiming = 3;
        public const int Skating = 4;
    }

    public class Player : Core
    {
        public static Player Instance;

        public int State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                    _state = value;
            }
        }

        private int _state = 0;

        public Player(GameObject model, Animator animator)
        {
            Instance = this;

            Add("PlayerTransform", new PlayerTransform(model));
            Add("PlayerAnimation", new PlayerAnimation(animator));
            Add("PlayerEventHandler", new PlayerEventHandler());
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