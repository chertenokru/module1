using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenuWINLoseScript : MonoBehaviour
{
    public string messageWin = "Вы выиграли";
    public string messageLose = "Вы проиграли";
    public Button buttonResetLevel;
    public Button buttonExitMenu;
    public TextMeshProUGUI textGameResult;
    public GameObject menu;
    private SceneLoadingLogic sceneLoadingLogic;

    // Start is called before the first frame update
    void Awake()
    {
        sceneLoadingLogic = FindObjectOfType<SceneLoadingLogic>();

        buttonExitMenu.onClick.AddListener(loadMenu);
        buttonResetLevel.onClick.AddListener(resetLevel);
        menu.SetActive(false);
    }

    public void ShowMenu(bool isWin)
    {
        textGameResult.text = isWin ? messageWin : messageLose;
        menu.SetActive(true);
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