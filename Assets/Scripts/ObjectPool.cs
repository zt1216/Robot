using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Prewarm(GameObject prefab,int count)
    {
        EnsureQueue(prefab);
        for(int i = 0;i<count;i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pools[prefab].Enqueue(obj);
        }
    }

    public GameObject Get(GameObject prefab,Vector3 pos,Quaternion rot,float lifetime = 0f)
    {
        EnsureQueue(prefab);
        GameObject obj;
        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
        }

        PoolItem item = obj.GetComponent<PoolItem>();
        if(item == null)
        {
            item = obj.AddComponent<PoolItem>();
        }
        item.prefab = prefab;
        item.lifetime = lifetime;

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        PoolItem item = obj.GetComponent<PoolItem>();
        if(item == null || item.prefab == null)
        {
            Destroy(obj);
            return;
        }
        obj.SetActive(false);
        pools[item.prefab].Enqueue(obj);
    }

    public static void Release(GameObject obj)
    {
        if(Instance != null)
        {
            Instance.Return(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
    void EnsureQueue(GameObject prefab)
    {
        if(!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }
    }
}
