using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;

public class IntroMenu : MonoBehaviour
{
    public PlayableDirector timeline;
    public Button pauseButton;
    public GameObject pauseMenu;
    public Slider volumeSlider;
    public Button mainMenuButton;
    public Button resumeButton;

    private bool isPaused = false;
    private float timeRemaining = 90f;
    private bool timerRunning = true;

    void Start()
    {
        pauseButton.onClick.AddListener(TogglePauseMenu);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        resumeButton.onClick.AddListener(Resume);
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                GoToMainMenu();
            }
        }
    }

    void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    void Pause()
    {
        timerRunning = false;
        timeline.Pause();
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        pauseButton.gameObject.SetActive(false);
    }

    void Resume()
    {
        timerRunning = true;
        timeline.Resume();
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Main Menu");
    }
}
