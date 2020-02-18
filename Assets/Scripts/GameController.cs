using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private List<Character> playerCharacters = new List<Character>();
    private List<Character> enemyCharacters = new List<Character>();
    Character currentTarget;
    bool waitingPlayerInput;


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

    void PlayerWon()
    {
        Debug.Log("Player won");
    }

    void PlayerLost()
    {
        Debug.Log("Player lost");
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
            PlayerLost();
            return true;
        }

        if (FirstAliveCharacter(enemyCharacters) == null)
        {
            PlayerWon();
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
    }
}