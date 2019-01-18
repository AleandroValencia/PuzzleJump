using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{

    [SerializeField] bool x, y, z;
    [SerializeField] bool clockwise = true;
    [SerializeField] bool oscillate = true;
    [SerializeField] float min = 0.0f;
    [SerializeField] float max = 360.0f;
    [SerializeField] AnimationCurve speed;
    Vector3 rotation;
    float currentRotation;
    float rotateAmount;
    bool oscillating = true;

    private void Awake()
    {
        speed.postWrapMode = WrapMode.Loop;
    }

    private void Start()
    {
        Vector3 _rotation = Vector3.zero;
        if (x)
            _rotation.x = 1;
        if (y)
            _rotation.y = 1;
        if (z)
            _rotation.z = -1;
        rotation = _rotation;
        currentRotation = min;
    }

    // Update is called once per frame
    void Update()
    {
        if (oscillate)
        {
            if (oscillating)
            {
                rotateAmount = speed.Evaluate(currentRotation);
                transform.Rotate(rotation, currentRotation);
                currentRotation+= 0.1f;
                if (currentRotation > max)
                {
                    oscillating = false;
                }
            }
            else
            {
                rotateAmount = speed.Evaluate(currentRotation);
                transform.Rotate(rotation, currentRotation);
                currentRotation-= 0.1f;
                if (currentRotation < min)
                {
                    oscillating = true;
                }
            }
        }
        else
        {
            rotateAmount = speed.Evaluate(currentRotation);
            transform.Rotate(rotation, rotateAmount);
            currentRotation++;
            if (currentRotation > 360.0f)
                currentRotation = 0.0f;
        }
    }
}
