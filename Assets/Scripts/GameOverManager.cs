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
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
    public void Show() 
    {
        if(gameOverPanel == null || enemySpawner == null || resultText == null)
        {
            return;
        }
        gameOverPanel.SetActive(true);
        isDead = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        resultText.text = "你死了  活到 Wave " + enemySpawner.CurrentWave;
    }
    public void RetryGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
