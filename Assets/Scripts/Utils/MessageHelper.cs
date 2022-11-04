using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public delegate void MessageHandler();
public delegate void MessageHandler<T>(T arg);
public delegate void MessageHandler<T1, T2>(T1 arg1, T2 arg2);
public delegate void MessageHandler<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
public delegate void MessageHandler<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

public static class MessageHelper
{
    private static Dictionary<string, Delegate> _messageTable = new Dictionary<string, Delegate>();

    public static void AddListener(string message, MessageHandler handler)
    {
        if (PreListenerAdding(message, handler))
        {
            _messageTable[message] = (MessageHandler) Delegate.Combine((MessageHandler) _messageTable[message], handler);
        }
    }

    public static void AddListener<T>(string message, MessageHandler<T> handler)
    {
        if (PreListenerAdding(message, handler))
        {
            _messageTable[message] = (MessageHandler<T>) Delegate.Combine((MessageHandler<T>) _messageTable[message], handler);
        }
    }

    public static void AddListener<T1, T2>(string message, MessageHandler<T1, T2> handler)
    {
        if (PreListenerAdding(message, handler))
        {
            _messageTable[message] = (MessageHandler<T1, T2>) Delegate.Combine((MessageHandler<T1, T2>) _messageTable[message], handler);
        }
    }

    public static void AddListener<T1, T2, T3>(string message, MessageHandler<T1, T2, T3> handler)
    {
        if (PreListenerAdding(message, handler))
        {
            _messageTable[message] = (MessageHandler<T1, T2, T3>) Delegate.Combine((MessageHandler<T1, T2, T3>) _messageTable[message], handler);
        }
    }

    public static void AddListener<T1, T2, T3, T4>(string message, MessageHandler<T1, T2, T3, T4> handler)
    {
        if (PreListenerAdding(message, handler))
        {
            _messageTable[message] = (MessageHandler<T1, T2, T3, T4>) Delegate.Combine((MessageHandler<T1, T2, T3, T4>) _messageTable[message], handler);
        }
    }

    public static void Broadcast(string message)
    {
        if (PreBroadcasting(message) && (_messageTable[message] != null))
        {
            MessageHandler source = _messageTable[message] as MessageHandler;
            if (source != null)
            {
                source();
            }
        }
    }

    public static void Broadcast<T>(string message, T arg)
    {
        if (PreBroadcasting(message) && (_messageTable[message] != null))
        {
            MessageHandler<T> source = _messageTable[message] as MessageHandler<T>;
            if (source != null)
            {
                source(arg);
            }
        }
    }

    public static void Broadcast<T1, T2>(string message, T1 arg1, T2 arg2)
    {
        if (PreBroadcasting(message) && (_messageTable[message] != null))
        {
            MessageHandler<T1, T2> source = _messageTable[message] as MessageHandler<T1, T2>;
            if (source != null)
            {
                source(arg1, arg2);
            }
        }
    }

    public static void Broadcast<T1, T2, T3>(string message, T1 arg1, T2 arg2, T3 arg3)
    {
        if (PreBroadcasting(message) && (_messageTable[message] != null))
        {
            MessageHandler<T1, T2, T3> source = _messageTable[message] as MessageHandler<T1, T2, T3>;
            if (source != null)
            {
                source(arg1, arg2, arg3);
            }
        }
    }

    public static void Broadcast<T1, T2, T3, T4>(string message, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (PreBroadcasting(message) && (_messageTable[message] != null))
        {
            MessageHandler<T1, T2, T3, T4> source = _messageTable[message] as MessageHandler<T1, T2, T3, T4>;
            if (source != null)
            {
                source(arg1, arg2, arg3, arg4);
            }
        }
    }

    private static void PostListenerRemoving(string message)
    {
        if (_messageTable.ContainsKey(message) && (_messageTable[message] == null))
        {
            _messageTable.Remove(message);
        }
    }

    private static bool PreBroadcasting(string message)
    {
        bool flag = true;
        if (!_messageTable.ContainsKey(message))
        {
            flag = false;
        }
        return flag;
    }

    private static bool PreListenerAdding(string message, Delegate listenerForAdding)
    {
        if (null == listenerForAdding)
        {
            return false;
        }

        bool flag = true;
        if (!_messageTable.ContainsKey(message))
        {
            _messageTable.Add(message, null);
        }
        Delegate delegate2 = _messageTable[message];
        if ((delegate2 != null) && (delegate2.GetType() != listenerForAdding.GetType()))
        {
            flag = false;
        }

        if (null != delegate2)
        {
            foreach (Delegate delegateCur in delegate2.GetInvocationList())
            {
                if (listenerForAdding == delegateCur)
                {
                    //已添加过，无需重复添加
                    return false;
                }
            }
        }

        return flag;
    }

    private static bool PreListenerRemoving(string message, Delegate listenerForRemoving)
    {
        bool flag = true;
        if (_messageTable.ContainsKey(message))
        {
            Delegate delegate2 = _messageTable[message];
            if (delegate2 == null)
            {
                flag = false;
                return flag;
            }
            if (delegate2.GetType() != listenerForRemoving.GetType())
            {
                flag = false;
            }
            return flag;
        }
        flag = false;
        return flag;
    }

    public static void RemoveListener(string message, MessageHandler handler)
    {
        if (PreListenerRemoving(message, handler))
        {
            _messageTable[message] = (MessageHandler) Delegate.Remove((MessageHandler) _messageTable[message], handler);
        }
        PostListenerRemoving(message);
    }

    public static void RemoveListener<T>(string message, MessageHandler<T> handler)
    {
        if (PreListenerRemoving(message, handler))
        {
            _messageTable[message] = (MessageHandler<T>) Delegate.Remove((MessageHandler<T>) _messageTable[message], handler);
        }
        PostListenerRemoving(message);
    }

    public static void RemoveListener<T1, T2>(string message, MessageHandler<T1, T2> handler)
    {
        if (PreListenerRemoving(message, handler))
        {
            _messageTable[message] = (MessageHandler<T1, T2>) Delegate.Remove((MessageHandler<T1, T2>) _messageTable[message], handler);
        }
        PostListenerRemoving(message);
    }

    public static void RemoveListener<T1, T2, T3>(string message, MessageHandler<T1, T2, T3> handler)
    {
        if (PreListenerRemoving(message, handler))
        {
            _messageTable[message] = (MessageHandler<T1, T2, T3>) Delegate.Remove((MessageHandler<T1, T2, T3>) _messageTable[message], handler);
        }
        PostListenerRemoving(message);
    }

    public static void RemoveListener<T1, T2, T3, T4>(string message, MessageHandler<T1, T2, T3, T4> handler)
    {
        if (PreListenerRemoving(message, handler))
        {
            _messageTable[message] = (MessageHandler<T1, T2, T3, T4>) Delegate.Remove((MessageHandler<T1, T2, T3, T4>) _messageTable[message], handler);
        }
        PostListenerRemoving(message);
    }

    public static void Release()
    {
        _messageTable.Clear();
    }
}

