#if false
namespace Snowfield
{
    public interface IComponentFetcher
    {
        T Get<T>() where T : class;
        Component GetByName(string name);
        void Add(string name, Component com);
        void Remove(string name);
    }

    public class Component
    {
        public string Name { get; set; }

        public IComponentFetcher ComFetcher = null;

        public virtual void Init(Core core)
        {
            _core = core;
        }

        public virtual void Close()
        { }

        public virtual void OnAdded()
        { }

        public virtual void OnRemoved()
        { }

        public T GetCom<T>() where T : class
        {
            return ComFetcher.Get<T>();
        }

        protected Core _core;
    }
}
#endif