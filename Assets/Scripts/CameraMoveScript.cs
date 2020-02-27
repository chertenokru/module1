﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveScript : MonoBehaviour
{
    private Camera camera;

    public float Y = 16;

    public float z = -10;
    public float speed = 2;

    private Transform target;
    private Vector3 targetPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        target = gameObject.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        targetPosition = target.position;
        targetPosition.y = Y;
        targetPosition.z += z;
        camera.transform.position = Vector3.Lerp(transform.position, targetPosition,Time.deltaTime*speed);
    }

    public void SetTarget(Transform transform)
    {
        target = transform;
    }
    
}
