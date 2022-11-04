#if false
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowfield
{
    public class ComponentContainer : IComponentFetcher
    {
        public Component[] All { get { return components.Values.ToArray(); } }

        public T Get<T>() where T : class
        {
            for (int i = 0; i < All.Length; i++)
            {
                var c = All[i];
                if (c is T)
                    return c as T;
            }

            return null;
        }

        public Component GetByName(string name)
        {
            if (components.ContainsKey(name))
                return components[name];

            return null;
        }

        public void Add(string name, Component c)
        {
            if (components.ContainsKey(name))
                throw new Exception("Component name conflicted: " + c.Name);

            c.Name = name;
            components[name] = c;
            c.ComFetcher = this;
            c.OnAdded();
        }

        public void Remove(string name)
        {
            if (! components.ContainsKey(name))
                return;

            var c = components[name];
            components.Remove(name);
            c.OnRemoved();
        }

        private Dictionary<string, Component> components = new Dictionary<string, Component>();
    }
}
#endif