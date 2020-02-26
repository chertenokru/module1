using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class Weapons
{
    public enum WeaponsType
    {
        [Description("Нету")]
        None,
        [Description("Пистолет")]
        Pistol,
        [Description("Бита")]
        Bat,
        [Description("Нож")]
        Knife
    }

    public WeaponsType weaponsType;
    public int damage;
    public GameObject meshObject;
    public bool isDistanceAttack;
    public float distanceToAttack = 3f;

}