using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class pausemanager : MonoBehaviour
{
    public GameObject pausePanel; // Assign in Inspector

    private bool isPaused = false;

    void Update()
    {
        // Optional: Pause game with Escape key (PC testing)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Unfreeze game
        isPaused = false;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f; // Ensure time is normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Ensure time is normal
        Application.Quit(); // Will not quit in Editor
    }
    public void GoToScene1()
    {
        Time.timeScale = 1f; // Resume time in case it's paused
        SceneManager.LoadScene("SampleScene"); // Make sure Scene 1 is in Build Settings
    }
  
}
