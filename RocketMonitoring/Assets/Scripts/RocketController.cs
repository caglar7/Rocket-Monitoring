using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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
    private Material[] rocketMaterials;

    [Header("Rocket Model Parts")]
    [SerializeField]
    private GameObject rocketFull;

    [Header("Parts Main Object")]
    [SerializeField]
    private GameObject rocketPart_All;

    [Header("Parts First Depart")]
    [SerializeField]
    private GameObject rocketPart_Bottom;
    [SerializeField]
    private GameObject rocketPart_MiddleTop;
    [SerializeField]
    private float forceMagnitudeFirst = 1f;

    [Header("Parts Second Depart")]
    [SerializeField]
    private GameObject rocketPart_Middle;
    [SerializeField]
    private GameObject rocketPart_Top;
    [SerializeField]
    private float forceMagnitudeSecond = 1f;

    // depart modeling
    private bool applyOnceFirst = false;
    private bool applyOnceSecond = false;
 
    

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

        // deactivate rocket parts
        rocketPart_All.gameObject.SetActive(false);
    }

    
    void Update()
    {
        if (rocketFull.activeInHierarchy == false)
            return;

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

        // check rotation to do open parachute test 
        if (Mathf.Abs(rollSet) >= 70f || Mathf.Abs(pitchSet) >= 70f)
            OpenFirstParachute();
    }

    void FixedUpdate()
    {
        if(applyOnceFirst)
        {
            applyOnceFirst = false;
            rocketPart_MiddleTop.GetComponent<Rigidbody>().AddForce(transform.up * forceMagnitudeFirst, ForceMode.Impulse);
            rocketPart_Bottom.GetComponent<Rigidbody>().AddForce(-1 * transform.up * forceMagnitudeFirst, ForceMode.Impulse);
        }
        if(applyOnceSecond)
        {
            applyOnceSecond = false;
            Vector3 secondForceDir = rocketPart_MiddleTop.transform.up;
            rocketPart_Top.GetComponent<Rigidbody>().AddForce(secondForceDir * forceMagnitudeSecond, ForceMode.Impulse);
            rocketPart_Middle.GetComponent<Rigidbody>().AddForce(-1 * secondForceDir * forceMagnitudeSecond, ForceMode.Impulse);
        }
    }

    public void RotateRocket(string rollString, string pitchString)
    {
        if (rocketFull.activeInHierarchy == false)
            return;

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
        if(rocketFull.activeInHierarchy)
            rocketFull.GetComponent<MeshRenderer>().material = rocketMaterials[index];

        // set all individual materials
        if (rocketPart_All.activeInHierarchy)
        {
            rocketPart_Bottom.GetComponent<MeshRenderer>().material = rocketMaterials[index];
            rocketPart_Middle.GetComponent<MeshRenderer>().material = rocketMaterials[index];
            rocketPart_Top.GetComponent<MeshRenderer>().material = rocketMaterials[index];
        }

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

    public void OpenFirstParachute()
    {
        // activate parts object
        rocketFull.gameObject.SetActive(false);
        rocketPart_All.gameObject.SetActive(true);

        // set proper materials
        SetRocketMaterial(materialSelected);

        // impulse to bottom and middletop part
        applyOnceFirst = true;
    }

    public void OpenSecondParachute()
    {
        rocketPart_Top.AddComponent<Rigidbody>();
        rocketPart_Middle.AddComponent<Rigidbody>();

        applyOnceSecond = true;
    }

    IEnumerator WaitAndOpenSecond()
    {
        yield return new WaitForSeconds(1f);
        OpenSecondParachute();
    }
}
