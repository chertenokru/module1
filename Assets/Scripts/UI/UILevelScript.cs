using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelScript : MonoBehaviour
{
    public Button buttonAttack;
    public Button buttonChangeTarget;
    private GameController gameController;


    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        buttonAttack.onClick.AddListener(gameController.PlayerMove);
        buttonChangeTarget.onClick.AddListener(gameController.SwitchCharacter);
    }

    public void ShowMenu(bool isVisible)
    {
        buttonAttack.gameObject.SetActive(isVisible);
        buttonChangeTarget.gameObject.SetActive(isVisible);
        
    }

}
