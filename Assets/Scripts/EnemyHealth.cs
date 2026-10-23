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
    private Material matInst;
    private Color originalColor;

    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    private Animator anim;
    private bool isDead = false;
    private Coroutine flashCo;
    private Material[] matInsts;
    private Color[] originalColors;


    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        Renderer[] rends = GetComponentsInChildren<Renderer>();
        matInsts = new Material[rends.Length];
        originalColors = new Color[rends.Length];
        for(int i = 0;i<rends.Length;i++)
        {
            matInsts[i] = rends[i].material;
            originalColors[i] = matInsts[i].color;
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
            currentHealth = 0;
            isDead = true;
            Die();
            StartCoroutine(DeathSequence());
        }
    }

    void Flash()
    {
        if(matInsts == null || matInsts.Length == 0)
        {
            return;
        }
        for(int i = 0;i<matInsts.Length;i++)
        {
            matInsts[i].color = flashColor;
        }

        if(flashCo != null)
        {
            StopCoroutine(flashCo);
        }
        flashCo = StartCoroutine(FlashBack());
    }

    IEnumerator FlashBack()
    {
        yield return new WaitForSeconds(flashDuration);
        if(matInsts != null)
        {
            for (int i = 0; i < matInsts.Length; i++)
            {
                matInsts[i].color = originalColors[i];
            }
        }
        flashCo = null;
    }
    void Die()
    {
        Collider[] cols = GetComponentsInChildren<Collider>();
        for(int i = 0;i<cols.Length;i++)
        {
            cols[i].enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if(agent != null)
        {
            agent.enabled = false;
        }
        EnemyAI ai = GetComponent<EnemyAI>();
        if(ai != null)
        {
            ai.enabled = false;
        }
        if(anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.SetFloat("Speed", 0f);
            anim.Play("Death");
        }
    }
    IEnumerator DeathSequence()
    {
        yield return null;
        float deathLength = 1.5f;
        if(anim != null)
        {
            deathLength = anim.GetCurrentAnimatorStateInfo(0).length;
        }
        yield return new WaitForSeconds(deathLength);

        if(explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }
        Destroy(gameObject);
    }
}
