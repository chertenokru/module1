using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Game,
        Win,
        Lose
    }

    public Shader selectShader;
    private List<Character> playerCharacters = new List<Character>();
    private List<Character> enemyCharacters = new List<Character>();
    private UIMenuWINLoseScript uiMenuWinLoseScript;
    private Character currentTarget;
    private bool waitingPlayerInput;
    private GameState gameState = GameState.Game;

    private void Awake()
    {
        uiMenuWinLoseScript = FindObjectOfType<UIMenuWINLoseScript>();
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Character character in FindObjectsOfType<Character>())
        {
            if (character.type == Character.CharacterType.PoliceMan)
                playerCharacters.Add(character);
            else
                enemyCharacters.Add(character);
        }

        StartCoroutine(GameLoop());
    }

    [ContextMenu("Player Move")]
    public void PlayerMove()
    {
        if (waitingPlayerInput)
            waitingPlayerInput = false;
    }

    [ContextMenu("Switch character")]
    public void SwitchCharacter()
    {
        for (int i = 0; i < enemyCharacters.Count; i++)
        {
            // Найти текущего персонажа (i = индекс текущего)
            if (enemyCharacters[i] == currentTarget)
            {
                int start = i;
                ++i;
                // Идем в сторону конца массива и ищем живого персонажа
                for (; i < enemyCharacters.Count; i++)
                {
                    if (enemyCharacters[i].IsDead())
                        continue;

                    // Нашли живого, меняем currentTarget
                    currentTarget.GetComponentInChildren<TargetIndicator>(true).gameObject.SetActive(false);
                    currentTarget = enemyCharacters[i];
                    currentTarget.GetComponentInChildren<TargetIndicator>(true).gameObject.SetActive(true);

                    return;
                }

                // Идем от начала массива до текущего и смотрим, если там кто живой
                for (i = 0; i < start; i++)
                {
                    if (enemyCharacters[i].IsDead())
                        continue;

                    // Нашли живого, меняем currentTarget
                    currentTarget.GetComponentInChildren<TargetIndicator>(true).gameObject.SetActive(false);
                    currentTarget = enemyCharacters[i];
                    currentTarget.GetComponentInChildren<TargetIndicator>(true).gameObject.SetActive(true);

                    return;
                }

                // Живых больше не осталось, не меняем currentTarget
                return;
            }
        }
    }

    void PlayerWin()
    {
        uiMenuWinLoseScript.ShowMenu(true);
    }

    void PlayerLost()
    {
        
        uiMenuWinLoseScript.ShowMenu(false);
    }

    Character FirstAliveCharacter(List<Character> characters)
    {
        foreach (var character in characters)
        {
            if (!character.IsDead())
                return character;
        }

        return null;
    }

    bool CheckEndGame()
    {
        if (FirstAliveCharacter(playerCharacters) == null)
        {
            gameState =  GameState.Lose;
            return true;
        }

        if (FirstAliveCharacter(enemyCharacters) == null)
        {
            gameState = GameState.Win;
            return true;
        }

        return false;
    }

    IEnumerator GameLoop()
    {
        while (!CheckEndGame())
        {
            foreach (var player in playerCharacters)
            {
                if (player.IsDead())
                    continue;

                Character target = FirstAliveCharacter(enemyCharacters);
                if (target == null)
                    break;

                currentTarget = target;
                currentTarget.GetComponentInChildren<TargetIndicator>(true).gameObject.SetActive(true);

                waitingPlayerInput = true;
                while (waitingPlayerInput)
                    yield return null;

                currentTarget.GetComponentInChildren<TargetIndicator>().gameObject.SetActive(false);

                player.SetTarget(currentTarget);
                player.Attack();
                while (!player.IsIdle())
                    yield return null;
            }

            foreach (var enemy in enemyCharacters)
            {
                if (enemy.IsDead())
                    continue;

                Character target = FirstAliveCharacter(playerCharacters);
                if (target == null)
                    break;

                enemy.SetTarget(target);
                enemy.Attack();
                while (!enemy.IsIdle())
                    yield return null;
            }
        }
        // аниматор танца
        List<Character> charList = (gameState == GameState.Lose) ? enemyCharacters : playerCharacters;
        foreach (Character character in charList)
        {
            if (!character.IsDead())
            {
                character.Win();
            }
        }

        // ждём 3 сек и выводим окно об окончании игры
        yield return new WaitForSeconds(3.0f);
        uiMenuWinLoseScript.ShowMenu(gameState == GameState.Win);

    }
}