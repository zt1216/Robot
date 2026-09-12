using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Text healthText;
    private float lastShown = -1f;
    public Image healthBar;
    private float lastShownBar = -1f;

    void Update()
    {
        if (playerHealth != null && healthText != null)
        {
            float current = playerHealth.CurrentHealth;
            if(Mathf.Abs(current - lastShown) > 0.1f)
            {
                healthText.text = Mathf.RoundToInt(current) + "/" + Mathf.RoundToInt(playerHealth.MaxHealth);
                lastShown = current;
            }
        }
        if (healthBar != null && playerHealth != null)
        {
            float ratio = playerHealth.CurrentHealth / playerHealth.MaxHealth;
            if(Mathf.Abs(ratio - lastShownBar) > 0.01f)
            {
                healthBar.fillAmount = ratio;
                if(ratio >= 0.6f)
                {
                    healthBar.color = Color.green;
                }
                else if(ratio >= 0.3f)
                {
                    healthBar.color = Color.yellow;
                }
                else
                {
                    healthBar.color = Color.red;
                }
                lastShownBar = ratio;
            }
        }
    }
}
