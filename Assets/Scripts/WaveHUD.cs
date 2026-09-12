using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveHUD : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public Text waveText;
    private int lastShown = -1;
    void Update()
    {
        if(enemySpawner == null || waveText == null)
        {
            return;
        }
        int current = enemySpawner.CurrentWave;
        if(current != lastShown)
        {
            waveText.text = "Wave " + current;
            lastShown = current;
        }
    }
}
