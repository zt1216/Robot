using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private Renderer rend;
    private Color originalColor;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Flash();
        if(currentHealth <= 0)
        {
            Die();
            Destroy(gameObject);
        }
    }

    void Flash()
    {
        if(rend == null)
        {
            return;
        }
        rend.material.color = flashColor;
        StartCoroutine(FlashBack());
    }

    IEnumerator FlashBack()
    {
        yield return new WaitForSeconds(flashDuration);
        if(rend != null)
        {
            rend.material.color = originalColor;
        }
    }
    void Die()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<EnemyAI>().enabled = false;
    }
}
