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
    private float timer = 0f;
    private Vector3 diffVector;

    // rb instances
    private Rigidbody rbObject;

    void Start()
    {
        refVector = Vector3.Cross(transform.up, Vector3.up).normalized;
        rbObject = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(isRotating)
        {
            transform.RotateAround(transform.position, refVector, angularVelocity * Time.deltaTime);

            if (transform.up.y >= 0.99f)
            {
                isRotating = false;
            }
        }

        if(isMoving)
        {
            timer += Time.deltaTime;
            transform.localPosition = transform.localPosition + (diffVector * Time.deltaTime / moveTime);

            if (timer >= moveTime)
                isMoving = false;
        }
    }



    public void ConnectToParachute(float delay, float time)
    {
        moveTime = time;
        StartCoroutine(WaitAndConnect(delay));
    }

    IEnumerator WaitAndConnect(float t)
    {
        yield return new WaitForSeconds(t);

        isMoving = true;
        payloadParachute = GameObject.FindWithTag("PayloadParachute");
        if (payloadParachute != null)
        {
            transform.parent = payloadParachute.transform;
            diffVector = Vector3.zero - transform.localPosition;
        }
    }

}
