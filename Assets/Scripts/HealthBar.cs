using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    public Gradient gradient;
    private Image fill;
    private Slider slider;
    private TextMeshProUGUI text;
    
    

    private void Awake()
    {
        slider = gameObject.GetComponent<Slider>();
        fill = slider.fillRect.GetComponentInChildren<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        fill.color = gradient.Evaluate(1f);
        text.SetText($"{slider.value}/{slider.maxValue}");
        
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);
        text.SetText($"{slider.value}/{slider.maxValue}");
    }
}
