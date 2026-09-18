using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public GameObject explosionPrefab;
    private Renderer rend;
    private Color originalColor;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    private Animator anim;
    private bool isDead = false;
    void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponentInChildren<Renderer>();
        anim = GetComponent<Animator>();
        if(rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        if(isDead)
        {
            return;
        }
        currentHealth -= damage;
        Flash();
        if(currentHealth <= 0)
        {
            isDead = true;
            Die();
            StartCoroutine(DeathSequence());
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
        anim.SetFloat("Speed", 0);
        anim.Play("Death");
    }
    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        GameObject fx = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
        Destroy(fx, 2f);
        Destroy(gameObject);
    }
}
