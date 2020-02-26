using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ZoneSelector : MonoBehaviour
{
    public enum CharacterType
    {
        Player,
        Target,
        Enemy
    }

    public GameObject moveZoneCursor;
    public GameObject attackZoneCursor;
    private Character character;
    private Vector3 pos;
    private Vector3 scale;

    void Awake()
    {
        HideZone();
    }

    public bool IsShow()
    {
        return moveZoneCursor.activeSelf;
    }

    public void ShowZone(CharacterType characterType, GameController.CharacterInfo characterinfo)
    {
        Character character = characterinfo.character;

        transform.position = character.transform.position;

        scale = moveZoneCursor.transform.localScale;
        // для игрока текущию доступную на данном ходу дистануию показываем, для цели - общию 
        scale.x =
            ((characterType != CharacterType.Target) ? character.distanceCurrentMove : character.distanceMaxMove) * 2;
        scale.y =
            ((characterType != CharacterType.Target) ? character.distanceCurrentMove : character.distanceMaxMove) * 2;
        moveZoneCursor.transform.localScale = scale;

        scale = attackZoneCursor.transform.localScale;
        scale.x = character.GetDistanceAttack() * 2;
        scale.y = character.GetDistanceAttack() * 2;
        attackZoneCursor.transform.localScale = scale;

        moveZoneCursor.SetActive(true);
        // для игрока может или нет аттаковать+наличие доступной цели,
        // для цели - всегда дистанцию аттаки
        if (characterType == CharacterType.Player)
            attackZoneCursor.SetActive(characterinfo.canFire && characterinfo.hasTarget);
        else
            attackZoneCursor.SetActive(true);
    }


    public void HideZone()
    {
        moveZoneCursor.SetActive(false);
        attackZoneCursor.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }
}