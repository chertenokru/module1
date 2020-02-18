using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPauseMenuScript : MonoBehaviour
{
    public Button buttonPause;
    public Button buttonContinue;
    public Button buttonResetLevel;
    public Button buttonExitMenu;

    public GameObject pauseMenu;
    private SceneLoadingLogic sceneLoadingLogic;

    // Start is called before the first frame update
    void Awake()
    {
        sceneLoadingLogic = FindObjectOfType<SceneLoadingLogic>();
        buttonPause.gameObject.SetActive(true);
        buttonPause.onClick.AddListener(ShowPause);
        buttonContinue.onClick.AddListener(ContinueGame);
        buttonExitMenu.onClick.AddListener(loadMenu);
        buttonResetLevel.onClick.AddListener(resetLevel);
        pauseMenu.SetActive(false);
    }

    private void ContinueGame()
    {
        buttonPause.gameObject.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void ShowPause()
    {
        buttonPause.gameObject.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0.00000000000001f;
    }

    public void loadMenu()
    {
        sceneLoadingLogic.LoadScene(SceneLoadingLogic.SceneNums.Menu);
    }

    public void resetLevel()
    {
        sceneLoadingLogic.ReloadCurrentScene();
    }
}