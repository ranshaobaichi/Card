using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public interface IObjectPoolCall<T>
{
    T Get();
    void Release(T obj);
}

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool instance;
    private Dictionary<string, IObjectPool<GameObject>> objectPools = new Dictionary<string, IObjectPool<GameObject>>();
    private Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();
    private GameObject poolRoot;

    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                // 查找现有的实例或创建一个新的
                instance = FindObjectOfType<ObjectPool>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ObjectPool");
                    instance = go.AddComponent<ObjectPool>();
                    // DontDestroyOnLoad(go); // 保持对象池在场景切换时不被销毁
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            poolRoot = gameObject;
            // DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // 获取对象
    public GameObject GetObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("尝试从对象池获取空prefab!");
            return null;
        }

        string key = prefab.name;

        // 确保子池存在
        if (!poolParents.TryGetValue(key, out Transform parent))
        {
            GameObject childPool = new GameObject(key + "Pool");
            childPool.transform.SetParent(transform);
            poolParents[key] = childPool.transform;
            parent = childPool.transform;
        }

        // 如果池子里有对象，直接取出
        if (objectPools.TryGetValue(key, out IObjectPool<GameObject> pool))
        {
            GameObject obj = pool.Get();
            return obj; // ObjectPool.Get()已激活对象
        }
        else
        {
            // 创建新对象池
            IObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
                createFunc: () => CreateNewObject(prefab, parent),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 50
            );

            objectPools[key] = newPool;
            GameObject obj = newPool.Get();
            return obj;
        }
    }

    // 创建新对象
    private GameObject CreateNewObject(GameObject prefab, Transform parent)
    {
        GameObject newObj = Instantiate(prefab, parent);
        newObj.name = prefab.name; // 避免 "(Clone)" 后缀
        return newObj;
    }

    // 回收对象
    public void PushObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        string key = obj.name;

        if (objectPools.TryGetValue(key, out IObjectPool<GameObject> pool))
        {
            pool.Release(obj); // Release会自动设置inactive
        }
        else
        {
            Destroy(obj);
        }
    }
}