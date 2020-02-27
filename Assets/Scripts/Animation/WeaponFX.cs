using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponFX : MonoBehaviour
{
    public ParticleSystem particleSystem;
    
    void Awake()
    {
        //particleSystem = GetComponent<ParticleSystem>();

    }

    public void Play()
    {
        particleSystem.Play();
    }
}
