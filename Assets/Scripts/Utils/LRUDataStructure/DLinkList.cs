public class Node<V,K> where V : class where K : class
{
    public V val;
    public K key;
    public Node<V,K> prev,next;
    public Node(V val,K key)
    {
        this.val = val;
        this.key = key;
    }
}
public class DLinkList<V,K> where V : class where K : class
{
    protected Node<V,K> head, tail; // 头尾虚节点

    public DLinkList() {
        head = new Node<V,K>(default(V),default(K));
        tail = new Node<V,K>(default(V),default(K));
        head.next = tail;
        tail.prev = head;
    }

    // 在链表头部添加节点 x
    public void AddFirst(Node<V,K> x) {
        if(x == head || x == tail || x == null || x.val == null)return;
        x.next = head.next;
        x.prev = head;
        head.next.prev = x;
        head.next = x;
    }

    // 删除链表中的 x 节点（x 一定存在）
    public void Remove(Node<V,K> x) {
        if(x == tail || x == head || x == null)return;
        x.prev.next = x.next;
        x.next.prev = x.prev;
    }
    
    // 删除链表中最后一个节点，并返回该节点
    public Node<V,K> RemoveLast() {
        if (tail.prev == head)
            return null;
        Node<V,K> last = tail.prev;
        Remove(last);
        return last;
    }
    
    public Node<V,K> GetLast() {
        if (tail.prev == head)
            return null;
        Node<V,K> last = tail.prev;
        return last;
    }
    public Node<V,K> GetFirst() {
        if (head.next == tail)
            return null;
        Node<V,K> first = head.next;
        return first;
    }

    public virtual void Clear()
    {
        head.next = tail;
        tail.prev = head;
    }
}