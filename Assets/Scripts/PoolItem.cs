using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolItem : MonoBehaviour
{
    [HideInInspector] public GameObject prefab;
    public float lifetime = 0f;

    private float timer;
    
    void OnEnable()
    {
        timer = lifetime;
    }

    void Update()
    {
        if(lifetime <= 0f || ObjectPool.Instance == null)
        {
            return;
        }
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            ObjectPool.Release(gameObject);
        }
    }
}
