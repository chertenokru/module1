using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoPanel : MonoBehaviour
{
    public Text name;
    public Text type;
    public Text health;
    public Text move;
    public Text weapon;
    public Text weaponDist;
    public Text weaponDamage;
    // Start is called before the first frame update

    public void Show(Character character)
    {
        if (character == null) return;
        name.text = character.name;
        type.text = character.type.ToString();
        health.text = $"{character.health}({character.maxHealth})";
        move.text = $"{character.distanceCurrentMove}({character.distanceMaxMove})";
        weapon.text = character.WeaponsType.ToString();
        weaponDist.text = character.GetDistanceAttack().ToString();
        weaponDamage.text = character.GetWeaponDamage().ToString();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public bool isShow()
    {
        return gameObject.activeSelf;
    }
}
