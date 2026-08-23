using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object를 생성하고 반납하는 곳을 담당하는 Class다.
/// </summary>
public class Factory
{
    private Dictionary<int, object> poolDictionary = new Dictionary<int, object>();
    private Dictionary<int, List<IEntity>> activeObjects = new Dictionary<int, List<IEntity>>();

    public T Create<T>(T prefab, Vector3 position, Quaternion rotation, float ScaleMultiplier = 1, Transform parent = null, int maxCount = 30) where T : Component, IEntity
    {
        // 생성할 때는 무조건 원본 Prefab이 들어올 것이기 때문에 id를 가져와도 된다.
        int key = prefab.gameObject.GetInstanceID();

        if(!poolDictionary.ContainsKey(key)) {
            poolDictionary.Add(key, new ObjectPool<T>(prefab, maxCount));
        }

        var pool = (ObjectPool<T>)poolDictionary[key];
        T poolingObject = pool.Get();
        poolingObject.SetObjectKey(key);
        poolingObject.OnSpawn();
        poolingObject.SetTransform(position, rotation, ScaleMultiplier, parent);
        poolingObject.gameObject.SetActive(true);

        if (!activeObjects.ContainsKey(key))
            activeObjects[key] = new List<IEntity>();

        activeObjects[key].Add(poolingObject);

        return poolingObject;
    }

    public void Release<T>(T instance) where T : Component, IEntity
    {
        int key = instance.GetObjectKey();

        if (activeObjects.ContainsKey(key))
            activeObjects[key].Remove(instance);

        if (poolDictionary.ContainsKey(key))
        {
            var pool = (ObjectPool<T>)poolDictionary[key];
            pool.Return(instance);
        }
        else
            GameObject.Destroy(instance.gameObject);
    }

    public void ReleaseAll()
    {
        foreach(var active in activeObjects)
        {
            var objects = new List<IEntity>(active.Value);

            foreach(var obj in objects)
            {
                obj.OnDespawn();
                (obj as Component)?.gameObject.SetActive(false);
            }

            active.Value.Clear();
        }
    }
}
