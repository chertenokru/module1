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
        public bool hasTarget = false;
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
            hasTarget = false;

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
    public Color selectOutlinePlayerColor = Color.yellow;
    public Color selectOutlineColor = Color.white;
    public Color selectOutlineTargetColor = Color.red;
    public int selectOutlineWith = 6;
    public GameObject characterInfoPanelPrefab;
    public GameObject damageIndicatorPrefab;

    private CharacterInfoPanel characterInfoPanel;
    private Dictionary<Character, CharacterInfo> playerCharacters = new Dictionary<Character, CharacterInfo>();
    private Dictionary<Character, CharacterInfo> enemyCharacters = new Dictionary<Character, CharacterInfo>();
    private List<CharacterInfo> targetList = new List<CharacterInfo>();
    private UIMenuWINLoseScript uiMenuWinLoseScript;
    private UILevelScript uiLevelScript;
    private Character currentTarget;
    private Character player;
    private Character enemy;
    private CharacterInfo currentTargetCharacterInfo;
    private CharacterInfo playerCharacterInfo;
    private CharacterInfo enemyCharacterInfo;
    private GameState gameState = GameState.Game;
    private ZoneSelector zoneSelectorTarget;
    private ZoneSelector zoneSelectorPlayer;

    private bool waitingPlayerInput;
    private bool waitingPlayerMove;


    private void Awake()
    {
        zoneSelectorPlayer = Instantiate(zoneSelectorPrefab).GetComponent<ZoneSelector>();
        zoneSelectorTarget = Instantiate(zoneSelectorPrefab).GetComponent<ZoneSelector>();
        GameObject obj = Instantiate(characterInfoPanelPrefab);
        characterInfoPanel = obj.GetComponent<CharacterInfoPanel>();
        characterInfoPanel.Hide();
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
        if (playerCharacterInfo != null && playerCharacterInfo.canMove) return true;

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
        foreach (Character character in charactersInfos.Keys.ToArray())
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
                FindTargetList(player, enemyCharacters, targetList);


                // подготовка
                playerCharacterInfo.TurnReset();

                if (targetList.Count > 0)
                {
                    currentTarget = targetList[0].character;
                    currentTargetCharacterInfo = targetList[0];
                    playerCharacterInfo.hasTarget = true;
                }
                else
                {
                    currentTarget = null;
                    currentTargetCharacterInfo = null;
                    playerCharacterInfo.hasTarget = false;
                }


                // включение всякого UI
                uiLevelScript.ShowMenu(true);
                zoneSelectorPlayer.ShowZone(ZoneSelector.CharacterType.Player, playerCharacterInfo);
                player.SwitchSelect(true, selectOutlinePlayerColor, selectOutlineWith);

                // ждём хода
                while (!playerCharacterInfo.isEndTurn)
                {
                    if (player.IsIdle() &&
                        playerCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterFire)
                    {
                        playerCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterIdle;
                        playerCharacterInfo.Update(false);
                        zoneSelectorPlayer.ShowZone(ZoneSelector.CharacterType.Player, playerCharacterInfo);
                        if (characterInfoPanel.isShow()) characterInfoPanel.Show(currentTarget);
                    }

                    if (player.IsIdle() &&
                        playerCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterMove)
                    {
                        playerCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterIdle;
                        //обновляем список доступных целей после передвижения
                        FindTargetList(player, enemyCharacters, targetList);
                        // есть ли цели
                        playerCharacterInfo.hasTarget = (targetList.Count > 0);
                        playerCharacterInfo.Update(playerCharacterInfo.canFire);
                        zoneSelectorPlayer.ShowZone(ZoneSelector.CharacterType.Player, playerCharacterInfo);
                    }

                    if ((!playerCharacterInfo.canFire ||
                         (playerCharacterInfo.canFire && !playerCharacterInfo.hasTarget)) &&
                        !playerCharacterInfo.canMove &&
                        playerCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterIdle)
                    {
                        playerCharacterInfo.isEndTurn = true;
                    }


                    yield return null;
                }

                // выключаем UI
                zoneSelectorPlayer.HideZone();
                characterInfoPanel.Hide();
                uiLevelScript.ShowMenu(false);
                player.SwitchSelect(false);
                player = null;
                playerCharacterInfo = null;
                if (currentTarget != null) currentTarget.SwitchSelect(false);
            }


            foreach (var _enemy in enemyCharacters)
            {
                enemy = _enemy.Key;
                if (enemy.IsDead()) continue;

                enemyCharacterInfo = _enemy.Value;
                enemyCharacterInfo.TurnReset();
                zoneSelectorPlayer.ShowZone(ZoneSelector.CharacterType.Enemy, enemyCharacterInfo);

                //FindTargetList(enemy.Key, playerCharacters, targetList);


                // ждём хода
                while (!enemyCharacterInfo.isEndTurn)
                {
                    if (enemy.IsIdle() &&
                        enemyCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterFire)
                    {
                        enemyCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterIdle;
                        enemyCharacterInfo.Update(false);
                        zoneSelectorPlayer.ShowZone(ZoneSelector.CharacterType.Enemy, enemyCharacterInfo);
                    }

                    if (enemy.IsIdle() &&
                        enemyCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterMove)
                    {
                        enemyCharacterInfo.characterState = CharacterInfo.CharacterState.CharacterIdle;
                        //обновляем список доступных целей после передвижения
                        enemyCharacterInfo.Update(playerCharacterInfo.canFire);
                        zoneSelectorPlayer.ShowZone(ZoneSelector.CharacterType.Enemy, enemyCharacterInfo);
                    }

                    if (!enemyCharacterInfo.canFire && !enemyCharacterInfo.canMove &&
                        enemyCharacterInfo.characterState == CharacterInfo.CharacterState.CharacterIdle)
                    {
                        enemyCharacterInfo.isEndTurn = true;
                    }

                    yield return null;
                    if (enemy.IsIdle() && !enemyCharacterInfo.isEndTurn) findEnemyTurn(enemy);
                }

                // выключаем UI
                zoneSelectorPlayer.HideZone();
                
                enemy.SwitchSelect(false);
                enemy = null;
                enemyCharacterInfo = null;
                if (currentTarget != null) currentTarget.SwitchSelect(false);
            }
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


    private void findEnemyTurn(Character enemyChar)
    {
        Vector3 posEnemy = enemyChar.transform.position;
        float distAttack = enemyChar.GetDistanceAttack();
        Character charMinDist = null;
        Character fireChar = null;
        float minDist = 0;
        FindMinDistOrFireDist(posEnemy, out charMinDist, out minDist);
        if (enemyCharacterInfo.canFire && minDist < distAttack)
        {
            SetTarget(enemyChar, charMinDist);
            Attack(enemyCharacterInfo, playerCharacters[charMinDist]);
        }
        else
            // если стрелять не можем и уже ходили то конец хода
        if (!enemyCharacterInfo.canMove) enemyCharacterInfo.isEndTurn = true;

        if (enemyCharacterInfo.canMove && charMinDist != null)
        {
            SetTarget(enemyChar, charMinDist);
            Move(enemyCharacterInfo, charMinDist.transform.position);
        }

        if (charMinDist == null) enemyCharacters[enemy].isEndTurn = true;
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
        if (playerCharacterInfo != null && !playerCharacterInfo.isEndTurn)
        {
            // персонаж?
            Character selCharacter = selObject.transform.gameObject.GetComponentInParent<Character>();
            if (selCharacter != null)
            {
                // чужой?  может быть выбран как цель
                if (enemyCharacters.ContainsKey(selCharacter) && !selCharacter.IsDead())
                {
                    // подсвечиваем
                    if (targetList.Contains(enemyCharacters[selCharacter]))
                        selCharacter.SwitchSelect(true, selectOutlineTargetColor, selectOutlineWith);
                    else
                        selCharacter.SwitchSelect(true, selectOutlineColor, selectOutlineWith);
                    zoneSelectorTarget.ShowZone(ZoneSelector.CharacterType.Target, enemyCharacters[selCharacter]);
                    characterInfoPanel.Show(selCharacter);
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

            characterInfoPanel.Hide();
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
            if (selCharacter != null && enemyCharacters.ContainsKey(selCharacter))
            {
                // чужой?  может быть выбран как цель
                if (targetList.Contains(enemyCharacters[selCharacter]))
                {
                    return Attack(playerCharacterInfo, enemyCharacters[selCharacter]);
                    ;
                }
            }
        }

        return false;
    }

    private bool Attack(CharacterInfo agressor, CharacterInfo target)
    {
        // если имеет право на выстрел и цель жива
        if (agressor.canFire && !target.character.IsDead())
        {
            zoneSelectorTarget.HideZone();
            // выбираем новой целью
            SetTarget(agressor.character, target.character);
            agressor.character.Attack();
            agressor.Update(false);
            agressor.characterState = CharacterInfo.CharacterState.CharacterFire;
            return true;
        }

        return false;
    }

    private void SetTarget(Character agressor, Character target)
    {
        if (currentTarget != null) currentTarget.SwitchSelect(false);
        currentTarget = target;
        if (target != null)
        {
            agressor.Target = target.transform;
            currentTarget.SwitchSelect(true, selectOutlineTargetColor, selectOutlineWith);
        }
    }

    public void Move(CharacterInfo characterInfo, Vector3 pointTo)
    {
        Character _character = characterInfo.character;
        if (characterInfo.canMove)
        {
            _character.Move(pointTo);
            characterInfo.Update(characterInfo.canFire);
            characterInfo.characterState = CharacterInfo.CharacterState.CharacterMove;
        }
    }

    public void CancelCharacterTurn()
    {
        playerCharacterInfo.EndTurn();
    }

    public void MovePlayer(Vector3 pointTo)
    {
        Move(playerCharacterInfo, pointTo);
    }
}