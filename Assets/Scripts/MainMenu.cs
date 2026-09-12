using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static bool loadSave = false;
    public void StartGame()
    {
        loadSave = false;
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        loadSave = true;
        SceneManager.LoadScene("GameScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
