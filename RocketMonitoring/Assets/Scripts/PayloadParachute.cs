using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadParachute : MonoBehaviour
{
    private Rigidbody rb;
    private bool isForceOn = false;
    private Vector3 forceVector;

    private bool isRotationOn = false;
    private Vector3 refVector;
    private float angularVelocity = 60f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(isRotationOn)
        {
            transform.RotateAround(transform.position, refVector, angularVelocity * Time.deltaTime);

            if (transform.up.y >= 0.97f)
            {
                isRotationOn = false;
            }
        }
    }

    void FixedUpdate()
    {
        // add constant force here on trigger
        if(isForceOn)
        {
            rb.AddForce(forceVector, ForceMode.Force);
        }
    }

    public void ApplyConstantForce(Vector3 force, float speed)
    {
        isForceOn = true;
        forceVector = force;

        isRotationOn = true;
        refVector = Vector3.Cross(transform.up, Vector3.up).normalized;
        angularVelocity = speed;
    }
}
