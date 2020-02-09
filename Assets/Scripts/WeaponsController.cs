using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsController : MonoBehaviour
{
    public Weapons[] Weaponses;

    public int GetDamage(Weapons.WeaponsType type)
    {
        foreach (var weapon in Weaponses)
        {
            if (weapon.weaponsType == type)
                return weapon.damage;
        }

        return 0;
    }


    public GameObject GetMeshWeapont(Weapons.WeaponsType type)
    {
        foreach (var weapon in Weaponses)
        {
            if (weapon.weaponsType == type)
                return weapon.meshObject;
        }

        return null;
    }
    
}