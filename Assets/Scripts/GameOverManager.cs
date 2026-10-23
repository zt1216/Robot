using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    private bool isDead = false;
    public EnemySpawner enemySpawner;
    public Text resultText;

    void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RetryGame();
            }
        }
    }
    public void Show() 
    {
        isDead = true;
        Time.timeScale = 0f;
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if(resultText != null && enemySpawner != null)
        {
            resultText.text = "你死了，活到Wave " + enemySpawner.CurrentWave;
        }
        PauseManager pm = FindObjectOfType<PauseManager>();
        if(pm != null)
        {
            pm.enabled = false; 
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void RetryGame()
    {
        Time.timeScale = 1f;
        MainMenu.loadSave = false;
        SceneManager.LoadScene("GameScene");
    }
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
