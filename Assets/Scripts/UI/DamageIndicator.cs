using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    public float speed = 5;
    public Color color1 = Color.red;
    public Color color2 = Color.yellow;
    public float timeColorChange = 0.5f;
    private float _timer = 0;

    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetDamage(int damage)
    {
        text.SetText(damage.ToString());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        if (_timer > timeColorChange)
        {
            text.color = (text.color == color1) ? color2 : color1;
            _timer = 0;
        }
        else
        {
            _timer += Time.deltaTime;
        }
    }
}