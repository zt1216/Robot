using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    public PlayerHealth playerHealth;
    private CanvasGroup cg;
    private float lastShown = -1f;
    private Coroutine flashCo;
    void Start()
    {
        cg = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if(playerHealth == null)
        {
            return;
        }
        float current = playerHealth.CurrentHealth;
        if(Mathf.Abs(current -  lastShown) > 0.1f)
        {
            if(current < lastShown)
            {
                if (flashCo != null)
                {
                    StopCoroutine(flashCo);
                }
                flashCo = StartCoroutine(FlashRoutine());
            }
        }
        lastShown = current; 
    }
    IEnumerator FlashRoutine()
    {
        cg.alpha = 0.5f;
        while(cg.alpha > 0f)
        {
            cg.alpha -= 2f * Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0f;
    }
}
