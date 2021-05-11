using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO.Ports;

// DATA ORDER AND INDEXES, IN STRING ARRAY
/*
    LAT     0
    LONG    1 
    A       2
    V       3
    AN      4
    FRT     5
    SND     6
    LAT_B   7
    LONG_B  8
*/

public class RocketController : MonoBehaviour
{
    // static variable that's assigned from DisplayData.cs
    public static RocketController instance;
    float rollCurrent = 0f, pitchCurrent = 0f;
    float rollPrev = 0f, pitchPrev = 0f;
    float rollDiff = 0f, pitchDiff = 0f;
    float rollSet = 0f, pitchSet = 0f;

    [SerializeField]
    [Header("Rocket Movement Elements")]
    float angleSetTime = 0.1f;
    float angularVelocity = 90f;


    // test delete later
    float rotateX = 0f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
    
    }

    
    void Update()
    {
        if (rollDiff == 0f && pitchDiff == 0f)
            return;

        // rotation increments
        rollSet += (rollDiff * Time.deltaTime / angleSetTime);
        pitchSet += (pitchDiff * Time.deltaTime / angleSetTime);

        // check if angle is reached
        float newRollDiff = rollCurrent - rollSet;
        float newPitchDiff = pitchCurrent - pitchSet;
        if((rollDiff * newRollDiff) <= 0f)
            rollSet = rollCurrent;
        if ((pitchDiff * newPitchDiff) <= 0f)
            pitchSet = pitchCurrent;

        // rotation
        transform.rotation = Quaternion.Euler(rollSet, 0f, pitchSet);

        Debug.Log("rollSet: " + rollSet + "     rollCurrent: " + rollCurrent);
    }

    public void RotateRocket(string rollString, string pitchString)
    {

        Debug.Log("############################################");

        rollPrev = rollCurrent;
        pitchPrev = pitchCurrent;

        rollCurrent = float.Parse(rollString) / 100f;
        pitchCurrent = float.Parse(pitchString) / 100f;
        
        rollDiff = rollCurrent - rollPrev;
        pitchDiff = pitchCurrent - pitchPrev;
        
        rollSet = rollPrev;
        pitchSet = pitchPrev;
    }
}
