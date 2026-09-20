using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    public float fallDamage = 9999f;
    void OnTriggerEnter(Collider other)
    {
        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(fallDamage);
            return;
        }
        EnemyHealth eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null)
        {
            eh.TakeDamage(fallDamage);
        }
    }
}
