// DataAdapter.cs
// Created by xiaojl Aug/15/2022
// 通过名称关联预制中对象的脚本

using System;
using System.Collections.Generic;
using UnityEngine;

public class DataAdapter : MonoBehaviour
{
    [Serializable]
    public class ItemData
    {
        public string Key;
        public GameObject Obj;

        public ItemData(string key, GameObject obj)
        {
            this.Key = key;
            this.Obj = obj;
        }
    }

    [SerializeField]
    private ItemData[] _items;

    private Dictionary<string, GameObject> _gameObjects;

    private void TryInit()
    {
        if (_gameObjects != null)
            return;

        _gameObjects = new Dictionary<string, GameObject>();
        if (_items != null && _items.Length > 0)
        {
            for (var i = 0; i < _items.Length; i++)
                _gameObjects.Add(_items[i].Key, _items[i].Obj);
        }
    }

    public GameObject FindGameObject(string key)
    {
        TryInit();

        GameObject obj;
        if (_gameObjects.TryGetValue(key, out obj))
            return obj;

        return null;
    }

    public T FindComponent<T>(string key) where T : Component
    {
        TryInit();

        var obj = FindGameObject(key);
        if (obj != null)
            return obj.GetComponent<T>();

        return null;
    }

    #region Editor Using
    public ItemData[] GetItems()
    {
        return _items;
    }

    public ItemData FindItem(string key)
    {
        if (_items == null || _items.Length == 0)
            return null;

        for (var i = 0; i < _items.Length; i++)
        {
            if (_items[i].Key == key)
                return _items[i];
        }

        return null;
    }

    public void AddItem(string key, GameObject obj)
    {
        if (_items == null)
            _items = new ItemData[0];

        if (FindItem(key) != null)
        {
            Debug.LogError("There already contains the same key of item");
            return;
        }

        var items = new ItemData[_items.Length + 1];
        _items.CopyTo(items, 0);

        items[items.Length - 1] = new ItemData(key, obj);

        _items = items;
    }

    public void DeleteItem(string key)
    {
        if (_items == null || _items.Length == 0)
            return;

        var items = new ItemData[_items.Length - 1];

        var index = 0;
        for (var i = 0; i < _items.Length; i++)
        {
            if (_items[i].Key == key)
                continue;

            items[index++] = _items[i];
        }

        _items = items;
    }

    public void ReplaceItem(string oldKey, string key, GameObject obj)
    {
        var item = FindItem(oldKey);
        if (item == null)
            return;

        if (item.Key == key)
        {
            item.Obj = obj;
        }
        else
        {
            if (FindItem(key) != null)
            {
                Debug.LogError("There already contains the same key of item");
                return;
            }

            item.Key = key;
            item.Obj = obj;
        }
    }
    #endregion
}
