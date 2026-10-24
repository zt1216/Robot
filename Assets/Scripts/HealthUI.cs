using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image whiteBar;
    public Image colorBar;
    public Text hpText;
    public PlayerHealth player;

    void Update()
    {
        float ratio = (float)player.CurrentHealth / player.maxHealth;
        colorBar.fillAmount = ratio;
        whiteBar.fillAmount = Mathf.MoveTowards(whiteBar.fillAmount, ratio, Time.deltaTime * 0.4f);

        colorBar.color = ratio > 0.3f ? new Color(0.49f, 0.847f, 0.545f) : new Color(0.89f, 0.29f, 0.29f);

        hpText.text = Mathf.RoundToInt(player.CurrentHealth) + "/" + Mathf.RoundToInt(player.maxHealth);
    }
}
