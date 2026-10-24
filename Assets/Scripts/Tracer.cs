using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour
{
    public float speed = 60f;
    private Vector3 target;
    public void FlyTo(Vector3 targetsPos)
    {
        target = targetsPos;
        transform.LookAt(target);
        StartCoroutine(Fly());
    }

    IEnumerator Fly()
    {
        while(Vector3.Distance(transform.position,target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.unscaledDeltaTime);
            yield return null;
        }
        ObjectPool.Release(gameObject);
    }
}
