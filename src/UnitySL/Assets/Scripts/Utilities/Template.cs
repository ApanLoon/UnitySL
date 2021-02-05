using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Pooled instancing system using scene templates </summary>
[Serializable]
public class Template<T> where T : MonoBehaviour
{
    public T template;
    public readonly List<T> items = new List<T>();
    public readonly Stack<T> pool = new Stack<T>();

    public void Initialize()
    {
        template.gameObject.SetActive(false);
    }

    public void Clear()
    {
        foreach (T item in items)
        {
            item.gameObject.SetActive(false);
            pool.Push(item);
        }
        items.Clear();
    }

    public void ReturnItemToPool(T item)
    {
        if (items.Remove(item))
        {
            item.gameObject.SetActive(false);
            pool.Push(item);
        }
        else
        {
            Debug.LogError("Trying to return an item to the pool which does not belong there.");
        }
    }

    public T InstantiateTemplate()
    {
        T obj;
        if (pool.Count > 0) obj = pool.Pop();
        else obj = GameObject.Instantiate(template);
        Transform t = obj.transform;
        t.SetParent(template.transform.parent);
        t.localPosition = Vector3.zero;
        t.localScale = Vector3.one;
        t.localRotation = Quaternion.identity;
        t.SetAsLastSibling();
        obj.gameObject.SetActive(true);
        items.Add(obj);
        return obj;
    }
}