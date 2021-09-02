using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadObject : MonoBehaviour
{
    private GameObject payloadParachute;

    // for rotation
    private float angularVelocity = 50f;
    private Vector3 refVector;
    private bool isRotating = true;

    // for parachute connection
    private bool isMoving = false;
    private float moveTime = 1f;

    void Start()
    {
        payloadParachute = GameObject.FindWithTag("PayloadParachute");
        refVector = Vector3.Cross(transform.up, Vector3.up).normalized;
    }

    void Update()
    {
        if(isRotating)
        {
            transform.RotateAround(transform.position, refVector, angularVelocity * Time.deltaTime);

            if (transform.up.y >= 0.97f)
            {
                isRotating = false;
            }
        }
    }

    void FixedUpdate()
    {
        if(isMoving)
        {

        }
    }

    public void ConnectToParachute(float time)
    {
        isMoving = true;
        moveTime = time;
    }

}
