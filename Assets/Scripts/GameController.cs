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


    public Color selectOutlineColor = Color.yellow;
    public Color selectOutlineTargetColor = Color.red;
    public int selectOutlineWith = 6;
    private List<Character> playerCharacters = new List<Character>();
    private List<Character> enemyCharacters = new List<Character>();
    private UIMenuWINLoseScript uiMenuWinLoseScript;
    private UILevelScript uiLevelScript;
    public Character currentTarget;
    public Character player;
    private bool waitingPlayerInput;
    private GameState gameState = GameState.Game;


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

        uiMenuWinLoseScript = FindObjectOfType<UIMenuWINLoseScript>();
        uiLevelScript = FindObjectOfType<UILevelScript>();

        uiLevelScript.ShowMenu(false);
        StartCoroutine(GameLoop());
    }

    public bool isMovingMode(out Character character)
    {
        character = player;
        if (waitingPlayerInput) return true;

        return false;
    }

    [ContextMenu("Player Move")]
    public void PlayerMove()
    {
        if (waitingPlayerInput)
        {
            waitingPlayerInput = false;
            uiLevelScript.ShowMenu(false);
        }
    }

    [ContextMenu("Switch character")]
    public void SwitchCharacter()
    {
        currentTarget.SwitchSelect(false);
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
                    SetTarget(enemyCharacters[i]);
                    return;
                }

                // Идем от начала массива до текущего и смотрим, если там кто живой
                for (i = 0; i < start; i++)
                {
                    if (enemyCharacters[i].IsDead())
                        continue;

                    // Нашли живого, меняем currentTarget
                    SetTarget(enemyCharacters[i]);
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
            gameState = GameState.Lose;
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
        //       yield return new WaitForSecondsRealtime(2.0f);
        //       print("корутина стартанула");
        while (!CheckEndGame())
        {
            foreach (var _player in playerCharacters)
            {
                if (_player.IsDead())
                    continue;

                Character target = FirstAliveCharacter(enemyCharacters);
                if (target == null)
                    break;

                uiLevelScript.ShowMenu(true);
                _player.distanceCurrentMove = _player.distanceMaxMove;
                _player.SwitchSelect(true, selectOutlineColor, selectOutlineWith);
                SetTarget(target);
                player = _player;


                waitingPlayerInput = true;
                while (waitingPlayerInput)
                    yield return null;
                player = null;
                _player.SwitchSelect(false);
                currentTarget.SwitchSelect(false);

                _player.SetTarget(currentTarget);
                _player.Attack();
                while (!_player.IsIdle())
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


    // выбран интерактивный объекет, надо как-то отреагировать чтоли...
    public bool MoveOnSelectebleObject(GameObject selObject)
    {
        if (waitingPlayerInput)
        {
            // персонаж?
            Character selCharacter = selObject.transform.gameObject.GetComponentInParent<Character>();
            if (selCharacter != null)
            {
                // чужой?  может быть выбран как цель
                if (enemyCharacters.Contains(selCharacter) && !selCharacter.IsDead())
                {
                    // подсвечиваем
                    selCharacter.SwitchSelect(true, selectOutlineTargetColor, selectOutlineWith);
                    return true;
                }
            }
        }

        return false;
    }

    // выбран интерактивный объекет, надо как-то отреагировать чтоли...
    public void MoveOffSelectebleObject(GameObject selObject)
    {
        //if (waitingPlayerInput)
        //{
        // персонаж?
        Character selCharacter = selObject.transform.gameObject.GetComponentInParent<Character>();
        if (selCharacter != null)
        {
            // не должен быть выбран?
            if (selCharacter != currentTarget && selCharacter != player)
            {
                // выключаем 
                selCharacter.SwitchSelect(false);
            }
        }

        //}
    }


    public bool ClickOnSelectebleCharacter(GameObject selObject)
    {
        if (waitingPlayerInput)
        {
            // персонаж?
            Character selCharacter = selObject.transform.gameObject.GetComponentInParent<Character>();
            if (selCharacter != null)
            {
                // чужой?  может быть выбран как цель
                if (enemyCharacters.Contains(selCharacter) && !selCharacter.IsDead())
                {
                    // выбираем новой целью
                    SetTarget(selCharacter);
                    return true;
                }
            }
        }

        return false;
    }


    private void SetTarget(Character target)
    {
        if (currentTarget != null) currentTarget.SwitchSelect(false);
        currentTarget = target;
        currentTarget.SwitchSelect(true, selectOutlineTargetColor, selectOutlineWith);
    }
}