using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ZoneSelector : MonoBehaviour
{
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

    public void ShowZone(Character character)
    {
        transform.position = character.transform.position;
        ;


        scale = moveZoneCursor.transform.localScale;
        scale.x = character.distanceCurrentMove * 2;
        scale.y = character.distanceCurrentMove * 2;
        moveZoneCursor.transform.localScale = scale;

        scale = attackZoneCursor.transform.localScale;
        scale.x = character.GetDistanceAttack() * 2;
        scale.y = character.GetDistanceAttack() * 2;
        attackZoneCursor.transform.localScale = scale;

        moveZoneCursor.SetActive(true);
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