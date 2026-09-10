using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private bool isDead = false;
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if(isDead)
        {
            return;
        }
        currentHealth -= damage;
        if(currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            Die();
        }
    }

    void Die()
    {
        PlayerController pc = GetComponent<PlayerController>();
        PlayerShooting ps = GetComponent<PlayerShooting>();
        GameOverManager gm = FindObjectOfType<GameOverManager>();
        if(pc != null && ps != null && gm != null)
        {
            pc.enabled = false;
            ps.enabled = false;
            gm.Show();
        }
        else
        {
            Debug.LogError("Die() 失败：缺组件 " +
            (pc == null ? "PlayerController " : "") +
            (ps == null ? "PlayerShooting " : "") +
            (gm == null ? "GameOverManager " : ""));
        }
    }
}
