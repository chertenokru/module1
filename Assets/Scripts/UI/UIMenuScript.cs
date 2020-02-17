using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMenuScript : MonoBehaviour
{
    public Button playLevel1Button;
    public Button playLevel2Button;
    public Button exitButton;
    public Scene scene1;
    public Scene scene2;

    private SceneLoadingLogic sceneLoadingLogic; 

    private void Awake()
    {
        playLevel1Button.onClick.AddListener(LoadLeve1);
        playLevel2Button.onClick.AddListener(LoadLevel2);
        exitButton.onClick.AddListener(GameExit);
        sceneLoadingLogic = GameObject.FindObjectOfType<SceneLoadingLogic>();
    }

    public void LoadLevel2()
    {
        sceneLoadingLogic.LoadScene(2);
    }

    public void GameExit()
    {
        Application.Quit(0);
    }

    public void LoadLeve1()
    {
        sceneLoadingLogic.LoadScene(1);
    }


}
