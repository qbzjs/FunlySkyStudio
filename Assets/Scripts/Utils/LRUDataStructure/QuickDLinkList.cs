using System.Collections.Generic;

public class QuickDLinkList<V,K> : DLinkList<V,K> where V : class where K : class
{
    private Dictionary<K,Node<V,K>> map;
    // private DLinkList<V,K> cache;
    
    public QuickDLinkList() {
        this.map = new Dictionary<K,Node<V,K>>();
        // this.cache = new DLinkList<V,K>();
    }
    
    public bool Find(K key)
    {
        return map.ContainsKey(key);
    }
    public void AddFirstQDNode(V v,K k)
    {
        AddFirstQDNode(new Node<V,K>(v,k));
    }
    private void AddFirstQDNode(Node<V,K> node)
    {
        AddFirst(node);
        if(!Find(node.key))
            map.Add(node.key,node);
        else
            map[node.key] = node;
    }
    public void DeleteQDNode(Node<V,K> node)
    {
        if(Find(node.key))
        {
            Remove(node);
            map.Remove(node.key);
        }
    }
    public V GetVal(K k)
    {
        if(!Find(k))return null;
        return map[k].val;
    }
    public Node<V,K> GetNode(K k)
    {
        if(!Find(k))return null;
        return map[k];
    }
    // public Node<V,K> GetLast()
    // {
    //     return cache.GetLast();
    // }
    // public Node<V,K> GetFirst()
    // {
    //     return cache.GetFirst();
    // }
    

    public override void Clear()
    {
        map.Clear();
        base.Clear();
    }

    
    public List<V> ToList()
    {
        var lruInfos = new List<V>();
        var p = GetFirst();
        while(p != null && p != GetLast().next)
        {
            lruInfos.Add(p.val);
            p = p.next;
        }
        return lruInfos;
    }
}
