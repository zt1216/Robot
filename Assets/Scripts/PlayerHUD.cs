using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Text healthText;
    private float lastShown = -1f;

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
    }
}
