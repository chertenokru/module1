using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameState
    {
        Game,
        Win,
        Lose
    }


    public GameObject zoneSelectorPrefab;
    public Color selectOutlineColor = Color.yellow;
    public Color selectOutlineTargetColor = Color.red;
    public int selectOutlineWith = 6;

    private List<Character> playerCharacters = new List<Character>();
    private List<Character> enemyCharacters = new List<Character>();
    private UIMenuWINLoseScript uiMenuWinLoseScript;
    private UILevelScript uiLevelScript;
    private Character currentTarget;
    private Character player;
    private GameState gameState = GameState.Game;
    private ZoneSelector zoneSelectorTarget;
    private ZoneSelector zoneSelectorPlayer;

    private bool waitingPlayerInput;
    private bool waitingPlayerMove;


    private void Awake()
    {
        zoneSelectorPlayer = Instantiate(zoneSelectorPrefab).GetComponent<ZoneSelector>();
        zoneSelectorTarget = Instantiate(zoneSelectorPrefab).GetComponent<ZoneSelector>();
    }

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

    public void PlayerMove()
    {
        if (waitingPlayerInput)
        {
            waitingPlayerInput = false;
            uiLevelScript.ShowMenu(false);
        }
    }

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

                // подготовка
                _player.distanceCurrentMove = _player.distanceMaxMove;

                // включение всякого UI
                uiLevelScript.ShowMenu(true);
                zoneSelectorPlayer.ShowZone(_player);
                _player.SwitchSelect(true, selectOutlineColor, selectOutlineWith);

                SetTarget(target);
                player = _player;


                // ждём хода
                waitingPlayerInput = true;
                while (waitingPlayerInput)
                {
                    yield return null;
                    // если ждём окончания хода, и игрок встал, то обновляем UI
                    if (waitingPlayerMove && player.IsIdle() && !player.navMeshAgent.hasPath)
                    {
                        waitingPlayerMove = false;
                        zoneSelectorPlayer.ShowZone(player);
                    }
                }

                // выключаем UI
                zoneSelectorPlayer.HideZone();
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

                // Character target = FirstAliveCharacter(playerCharacters);
                //if (target == null)
                //    break;

                //zoneSelectorTarget.ShowZone(enemy);
                enemy.distanceCurrentMove = enemy.distanceMaxMove;
                findEnemyTurn(enemy);
                while (!enemy.IsIdle() && !enemy.navMeshAgent.hasPath)
                    yield return null;
                zoneSelectorTarget.HideZone();
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


    private void findEnemyTurn(Character enemy)
    {
        Vector3 posEnemy = enemy.transform.position;
        float distAttack = enemy.GetDistanceAttack();
        Character charMinDist = null;
        Character fireChar = null;
        float minDist = 0;
        FindMinDistOrFireDist(posEnemy, out charMinDist, out minDist);
        if (minDist < distAttack)
        {
            enemy.SetTarget(charMinDist);
            enemy.Attack();
        }
        else if (minDist < distAttack + enemy.distanceCurrentMove)
        {
            enemy.SetTarget(charMinDist);
            enemy.Move(charMinDist.transform.position);
        }
        else
        {
            enemy.SetTarget(charMinDist);
            enemy.Move(charMinDist.transform.position);
        }
    }


    private void FindMinDistOrFireDist(Vector3 posEnemy, out Character charMinDist, out float minDist)
    {
        minDist = Single.PositiveInfinity;
        charMinDist = null;
        // посчитаем дистанцию до каждого врага
        foreach (Character playerCharacter in playerCharacters)
        {
            if (!playerCharacter.IsDead())
            {
                float dist = Vector3.Distance(posEnemy, playerCharacter.transform.position);

                if (minDist > dist)
                {
                    minDist = dist;
                    charMinDist = playerCharacter;
                }
            }
        }
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
                    zoneSelectorTarget.ShowZone(selCharacter);
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

            zoneSelectorTarget.HideZone();
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
                    zoneSelectorTarget.HideZone();
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

    public void MovePlayer(Vector3 point)
    {
        if (waitingPlayerInput)
        {
            player.Move(point);
            waitingPlayerMove = true;
        }
    }
}