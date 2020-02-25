using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelScript : MonoBehaviour
{
    
    public Button buttonCancelTurn;
    private GameController gameController;


    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        buttonCancelTurn.onClick.AddListener(gameController.CancelCharacterTurn);
    }

    public void ShowMenu(bool isVisible)
    {
        buttonCancelTurn.gameObject.SetActive(isVisible);
        
    }

    
}
