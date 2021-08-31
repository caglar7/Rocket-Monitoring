using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// code to give a natural floating look to rocket parts 

public enum RocketPart
{
    MiddleTop,
    Bottom,
    Top,
    Middle
}

public class RocketPartMovement : MonoBehaviour
{
    [Header("Assign Rocket Part")]
    [SerializeField]
    private RocketPart currentRocketPart;

    [Header("Reference Point")]
    [SerializeField]
    private Transform referencePoint;

    [Header("Initial Rotation Angle and Time")]
    [SerializeField]
    private float initRotation = 60f;
    [SerializeField]
    private float initSetTime = 3f;
    private float initTimer= 0f;
    private bool initialMovement = true;

    private bool getReferenceVector = false;
    private Vector3 refVector;

    void Update()
    {
        if(RocketController.isMiddleTopMoving)
        {
            if(getReferenceVector == false)
            {
                getReferenceVector = true;
                refVector = Vector3.Cross(transform.up, Vector3.up).normalized; 
            }

            if(initialMovement)
            {
                
                // do initial rotation here for set time
                initTimer += Time.deltaTime;
                transform.RotateAround(referencePoint.position, -1 * refVector, initRotation * Time.deltaTime / initSetTime);

                if (initTimer >= initSetTime)
                    initialMovement = false;

            }
            else
            {
                // after initial rotation, natural floating here
            }

        }
    }
}
