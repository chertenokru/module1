using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
    
    
    public class CharacterInfo
    {
        
        public enum CharacterState
        {
            CharacterMove,
            CharacterFire,
            CharacterIdle
        }

        
        public Character character;
        public bool canMove = true;
        public bool canFire = true;
        public bool isEndTurn = false;
        public CharacterState characterState = CharacterState.CharacterIdle;

        public CharacterInfo(Character _character)
        {
            character = _character;
        }

        public void TurnReset()
        {
            canFire = true;
            canMove = true;
            isEndTurn = false;
            character.TurnReset();
            characterState = CharacterState.CharacterIdle;
        }

        public void EndTurn()
        {
            canFire = false;
            canMove = false;
            isEndTurn = true;
        }

        public void Update(bool _canFire)
        {
            canFire = _canFire;
            canMove = (character.distanceCurrentMove > 0.1f);
        }
    }


    public GameObject zoneSelectorPrefab;
    public Color selectOutlineColor = Color.yellow;
    public Color selectOutlineTargetColor = Color.red;
    public int selectOutlineWith = 6;

    private Dictionary<Character, CharacterInfo> playerCharacters = new Dictionary<Character, CharacterInfo>();
    private Dictionary<Character, CharacterInfo> enemyCharacters = new Dictionary<Character, CharacterInfo>();
    private List<CharacterInfo> targetList = new List<CharacterInfo>();
    private UIMenuWINLoseScript uiMenuWinLoseScript;
    private UILevelScript uiLevelScript;
    private Character currentTarget;
    private Character player;
    private CharacterInfo currentTargetCharacterInfo;
    private CharacterInfo playerCharacterInfo;
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
                playerCharacters.Add(character, new CharacterInfo(character));
            else
                enemyCharacters.Add(character, new CharacterInfo(character));
        }

        uiMenuWinLoseScript = FindObjectOfType<UIMenuWINLoseScript>();
        uiLevelScript = FindObjectOfType<UILevelScript>();

        uiLevelScript.ShowMenu(false);
        StartCoroutine(GameLoop());
    }

    public bool isMovingMode(out Character character)
    {
        character = player;
        if (playerCharacterInfo.canMove) return true;

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


    void PlayerWin()
    {
        uiMenuWinLoseScript.ShowMenu(true);
    }

    void PlayerLost()
    {
        uiMenuWinLoseScript.ShowMenu(false);
    }

    CharacterInfo FirstAliveCharacter(Dictionary<Character, CharacterInfo> charactersInfos)
    {
        // убираем трупы
        foreach (Character character in charactersInfos.Keys)
        {
            if (character.IsDead()) charactersInfos.Remove(character);
        }

        // остался кто?
        if (charactersInfos.Count > 0) return charactersInfos.First().Value;

        return null;
    }

    private void FindTargetList(Character character, Dictionary<Character, CharacterInfo> charactersInfos,
        List<CharacterInfo> targetList)
    {
        targetList.Clear();
        foreach (Character _character in charactersInfos.Keys)
        {
            if (!_character.IsDead() &&
                (Vector3.Distance(_character.transform.position, character.transform.position) <=
                 character.GetDistanceAttack())
            )
            {
                targetList.Add(charactersInfos[_character]);
            }
        }
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
                player = _player.Key;
                playerCharacterInfo = _player.Value;
                FindTargetList(player, playerCharacters, targetList);

                // подготовка
                playerCharacterInfo.TurnReset();

                if (targetList.Count > 0)
                {
                    currentTarget = targetList[0].character;
                    currentTargetCharacterInfo = targetList[0];
                }
                else
                {
                    currentTarget = null;
                    currentTargetCharacterInfo = null;
                    playerCharacterInfo.canFire = false;
                }


                // включение всякого UI
                uiLevelScript.ShowMenu(true);
                zoneSelectorPlayer.ShowZone(player);
                player.SwitchSelect(true, selectOutlineColor, selectOutlineWith);

                // ждём хода
                while (!playerCharacterInfo.isEndTurn)
                {
                    if (player.IsIdle() &&
                        playerCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterFire)
                    {
                        playerCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterIdle;
                        playerCharacterInfo.Update(false);
                        zoneSelectorPlayer.ShowZone(player);
                    }
                    if (player.IsIdle() &&
                        playerCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterMove)
                    {
                        playerCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterIdle;
                        playerCharacterInfo.Update(false);
                        zoneSelectorPlayer.ShowZone(player);
                    }

                    if (!playerCharacterInfo.canFire && !playerCharacterInfo.canMove &&
                        playerCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterIdle)
                    {
                        playerCharacterInfo.isEndTurn = true;
                    }
                    

                    yield return null;
                }

                // выключаем UI
                zoneSelectorPlayer.HideZone();
                player.SwitchSelect(false);
                player = null;
                if (currentTarget != null) currentTarget.SwitchSelect(false);
            }

            /*
            foreach (var enemy in enemyCharacters)
            {
                if (enemy.character.IsDead())
                    continue;
                zoneSelectorTarget.ShowZone(enemy.character);
                enemy.TurnReset();
                findEnemyTurn(enemy.character);
                while (!enemy.isEndTurn)
                    yield return null;
                zoneSelectorTarget.HideZone();
            }
            */
        }

        // аниматор танца
        List<Character> charList = (gameState == GameState.Lose)
            ? enemyCharacters.Keys.ToList()
            : playerCharacters.Keys.ToList();
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
        foreach (Character playerCharacter in playerCharacters.Keys)
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
        if (!playerCharacterInfo.isEndTurn)
        {
            // персонаж?
            Character selCharacter = selObject.transform.gameObject.GetComponentInParent<Character>();
            if (selCharacter != null)
            {
                // чужой?  может быть выбран как цель
                if (enemyCharacters.ContainsKey(selCharacter) && !selCharacter.IsDead())
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
        if (!playerCharacterInfo.isEndTurn)
        {
            // персонаж?
            Character selCharacter = selObject.transform.gameObject.GetComponentInParent<Character>();
            if (selCharacter != null)
            {
                // чужой?  может быть выбран как цель
                if (targetList.Contains(enemyCharacters[selCharacter]) && !selCharacter.IsDead())
                {
                    zoneSelectorTarget.HideZone();
                    // выбираем новой целью
                    SetTarget(selCharacter);
                    player.Attack();
                    playerCharacterInfo.Update(false);
                    playerCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterFire;
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
        if (playerCharacterInfo.canMove)
        {
            player.Move(point);
            playerCharacterInfo.Update(playerCharacterInfo.canFire);
            playerCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterMove;
        }
    }

    public void CancelCharacterTurn()
    {
        playerCharacterInfo.EndTurn();
    }
}