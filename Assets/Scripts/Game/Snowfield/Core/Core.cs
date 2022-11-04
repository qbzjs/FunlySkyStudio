#if false
namespace Snowfield
{
    public class Core
    {
        public virtual void Initialize()
        {
            foreach (var c in _cc.All)
                c.Init(this);
        }

        public virtual void Close()
        {
            foreach (var c in _cc.All)
                c.Close();
        }

        public virtual void Update(float deltaTime)
        {
            Component[] arr = _cc.All;
            for (int i = 0; i < arr.Length; i++)
            {
                Component c = arr[i];

                if (c is IFrameUpdate)
                    (c as IFrameUpdate).OnUpdate(deltaTime);
            }
        }

        public virtual void FixedUpdate(float fixedDeltaTime)
        {
            Component[] arr = _cc.All;
            for (int i = 0; i < arr.Length; i++)
            {
                Component c = arr[i];

                if (c is IFrameFixedUpdate)
                    (c as IFrameFixedUpdate).OnFixedUpdate(fixedDeltaTime);
            }
        }

        public virtual void LateUpdate()
        {
            Component[] arr = _cc.All;
            for (int i = 0; i < arr.Length; i++)
            {
                Component c = arr[i];

                if (c is IFrameLateUpdate)
                    (c as IFrameLateUpdate).OnLateUpdate();
            }
        }

        public T Get<T>() where T : class
        {
            return _cc.Get<T>();
        }

        public Component GetByName(string name)
        {
            return _cc.GetByName(name);
        }

        public void Add(string name, Component c)
        {
            _cc.Add(name, c);
        }

        public void Remove(string name)
        {
            _cc.Remove(name);
        }

        private ComponentContainer _cc = new ComponentContainer();
    }
}
#endif