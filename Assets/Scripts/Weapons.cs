using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Weapons
{
    public enum WeaponsType
    {
        None,
        Pistol,
        Bat,
        Knife
    }

    public WeaponsType weaponsType;
    public int damage;
    public GameObject meshObject;
   
}