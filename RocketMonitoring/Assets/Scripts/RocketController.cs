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
    public int materialSelected = 0;
     
    private float rollCurrent = 0f, pitchCurrent = 0f;
    private float rollPrev = 0f, pitchPrev = 0f;
    private float rollDiff = 0f, pitchDiff = 0f;
    private float rollSet = 0f, pitchSet = 0f;

    private float angleSetTime = 1f;

    // rotation only happens between specific angles, 2, 4, 6, 8 etc.
    private float roundAngle = 3f;

    [Header("Rocket Materials")]
    [SerializeField]
    Material[] rocketMaterials;
    

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        // set initial material
        angleSetTime = EntryManager.dataObtainPeriod;
        SetRocketMaterial(0);
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
    }

    public void RotateRocket(string rollString, string pitchString)
    {
        rollPrev = rollCurrent;
        pitchPrev = pitchCurrent;

        rollCurrent = float.Parse(rollString) / 100f;
        pitchCurrent = float.Parse(pitchString) / 100f;

        // round to proper angles
        rollCurrent = RoundAngle(rollCurrent);
        pitchCurrent = RoundAngle(pitchCurrent);

        rollDiff = rollCurrent - rollPrev;
        pitchDiff = pitchCurrent - pitchPrev;
        
        rollSet = rollPrev;
        pitchSet = pitchPrev;
    }

    // index 0 for glow, 1 for hull material
    public void SetRocketMaterial(int index)
    {
        GetComponent<MeshRenderer>().material = rocketMaterials[index];
        materialSelected = index;
        LogManager.instance.SendMessageToLog("Rocket Model " + index + " is selected");
    }

    private float RoundAngle(float angle)
    {
        float modValue = angle % roundAngle;
        if (modValue == 0f)
            return angle;

        float signedRoundAngle = (angle > 0f) ? (roundAngle) : (-roundAngle);
        float value = (Mathf.Abs(modValue) >= (roundAngle / 2f)) ? (signedRoundAngle - modValue) : (-modValue);
        return angle + value;
    }
}
