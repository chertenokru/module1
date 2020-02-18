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
    private SceneLoadingLogic sceneLoadingLogic; 

    private void Awake()
    {
        playLevel1Button.onClick.AddListener(LoadLeve1);
        playLevel2Button.onClick.AddListener(LoadLevel2);
        exitButton.onClick.AddListener(GameExit);
          var p =  FindObjectsOfType<SceneLoadingLogic>();
          sceneLoadingLogic = p[p.Length-1];
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
