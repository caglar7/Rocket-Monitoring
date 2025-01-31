﻿using System.Collections;
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

// when rotating kinematic body, might use fixed joint parts for better illustration

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

    [Header("Camera Animator")]
    [SerializeField]
    private Animator animatorCamera;

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

    [Header("First Depart Objects")]
    [SerializeField]
    private Rigidbody rbMiddleTop;
    [SerializeField]
    private Rigidbody rbMiddleTopFixed;
    [SerializeField]
    private Rigidbody rbBottom;
    [SerializeField]
    private Rigidbody[] rbFirstRope;
    [SerializeField]
    private float setFirstKinematicTime = 1f;
    [SerializeField]
    private GameObject firstParachutePrefab;

    [Header("First Depart Payload Parachute")]
    [SerializeField]
    private GameObject payloadParachutePrefab;
    [SerializeField]
    private float forceMagPayloadParachute = 10f;
    [SerializeField]
    private float payloadImpulseMult = 1f;
    [SerializeField]
    private float setForceDirY = .1f;

    [Header("First Depart Payload Object")]
    [SerializeField]
    private GameObject payloadPrefab;
    [SerializeField]
    private float forceMagPayloadObject = 10f;

    // get rid of every shadow
    // hide fixed meshes used for rotation pivot
    // move camera back smoothly
    [Header("Second Depart Objects")]
    [SerializeField]
    private GameObject sphere_FixedTop;
    [SerializeField]
    private GameObject[] secondRope;
    [SerializeField]
    private float setSecondKinematicTime = 1f;
    [SerializeField]
    private GameObject secondParachutePrefab;

    [Header("Departing Angle")]
    [SerializeField]
    private float departingAngle = 75f;
    [SerializeField]
    private float departAngleSetTime = 1.2f;

    [Header("Explosions")]
    [SerializeField]
    private GameObject openingExplosion;
    [SerializeField]
    private Transform firstOpeningTransform;
    [SerializeField]
    private Transform secondOpeningTransform;

    // depart modeling
    private bool applyOnceFirst = false;
    private bool applyOnceSecond = false;
    private bool applyOncePayloadParachute = false;
    private bool applyOncePayloadObject = false;

    private float departRollAngle = 0f;
    private float departPitchAngle = 0f;

    private bool isDeparting = false;
    private float departTimer;

    // rocket parts individual floating movement check
    public static bool isMiddleTopMoving = false;
    public static bool isBottomMoving = false;
    public static bool isTopMoving = false;

    // parachute bools
    private bool openFirst = false;
    private bool openSecond = false;

    // booster effect
    public ParticleSystem boosterEffect;
 
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

        // get particle system for booster
        boosterEffect = rocketFull.gameObject.GetComponentInChildren<ParticleSystem>();

    }
 
    void Update()
    {
        if(isDeparting)
        {
            departTimer += Time.deltaTime;
            if (departTimer >= setFirstKinematicTime)
                isDeparting = false;
            else
                return;
        }

        if (rocketFull.activeInHierarchy == false && Mathf.Abs(rollSet) == departingAngle || Mathf.Abs(pitchSet) == departingAngle)
        {
            return;
        }

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

    void FixedUpdate()
    {
        if (applyOnceFirst)
        {
            applyOnceFirst = false;
            rocketPart_MiddleTop.GetComponent<Rigidbody>().AddForce(transform.up * forceMagnitudeFirst, ForceMode.Impulse);
            rocketPart_Bottom.GetComponent<Rigidbody>().AddForce(-1 * transform.up * forceMagnitudeFirst, ForceMode.Impulse);

            // explosion here
            GameObject explosion1 = Instantiate(openingExplosion, firstOpeningTransform.position, Quaternion.identity);
            explosion1.transform.eulerAngles = rocketPart_MiddleTop.transform.eulerAngles;
        }

        if(applyOnceSecond)
        {
            applyOnceSecond = false;
            Vector3 secondForceDir = rocketPart_MiddleTop.transform.up;
            rocketPart_Top.GetComponent<Rigidbody>().AddForce(secondForceDir * forceMagnitudeSecond, ForceMode.Impulse);

            // explosion here
            GameObject explosion2 = Instantiate(openingExplosion, secondOpeningTransform.position, Quaternion.identity);
            explosion2.transform.eulerAngles = rocketPart_Top.transform.eulerAngles;
        }

        if(applyOncePayloadParachute)
        {
            applyOncePayloadParachute = false;
            Vector3 forceDir = -1 * rocketPart_MiddleTop.transform.up;
            forceDir.y = setForceDirY;

            GameObject payloadParachute = Instantiate(payloadParachutePrefab, rbMiddleTopFixed.gameObject.transform.position, Quaternion.identity);
            payloadParachute.transform.eulerAngles = -1 * rocketPart_MiddleTop.transform.eulerAngles;

            // reach script of parachute and add force that way,
            // script will rotate parachute aswell
            payloadParachute.GetComponent<Rigidbody>().AddForce(forceDir * forceMagPayloadParachute * payloadImpulseMult, ForceMode.Impulse);
            payloadParachute.GetComponent<PayloadParachute>().ApplyConstantForce(forceDir * forceMagPayloadParachute, 150f);
        }

        if(applyOncePayloadObject)
        {
            applyOncePayloadObject = false;

            Vector3 startPos = rbFirstRope[rbFirstRope.Length - 1].gameObject.transform.position;
            GameObject payloadObject = Instantiate(payloadPrefab, startPos, Quaternion.identity);
            payloadObject.transform.eulerAngles = rocketPart_Bottom.transform.eulerAngles;

            payloadObject.GetComponent<PayloadObject>().ConnectToParachute(0.2f, 0.4f);
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

        // clamp values
        rollCurrent = Mathf.Clamp(rollCurrent, -90f, 90f);
        pitchCurrent = Mathf.Clamp(pitchCurrent, -90f, 90f);

        // round to proper angles
        rollCurrent = RoundAngle(rollCurrent);
        pitchCurrent = RoundAngle(pitchCurrent);

        rollDiff = rollCurrent - rollPrev;
        pitchDiff = pitchCurrent - pitchPrev;
        
        rollSet = rollPrev;
        pitchSet = pitchPrev;
    }

    private void RotateDepartedRocket(float roll, float pitch)
    {
        // clamp values
        roll = Mathf.Clamp(roll, -90f, 90f);
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        angleSetTime = departAngleSetTime;

        rollPrev = transform.rotation.eulerAngles.x;
        pitchPrev = transform.rotation.eulerAngles.z;

        rollPrev = (rollPrev >= 270f) ? (rollPrev - 360f) : rollPrev;
        pitchPrev = (pitchPrev >= 270f) ? (pitchPrev - 360f) : pitchPrev;

        rollCurrent = roll;
        pitchCurrent = pitch;

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

    #region Open First Parachute
    public void OpenFirstParachute()
    {
        if(openFirst == false)
        {
            // check angles first
            if (Mathf.Abs(rollSet) <= 15f || Mathf.Abs(pitchSet) <= 15f)
            {
                angleSetTime = .4f;
                RotateRocket("0.00", "-30.00");
                StartCoroutine(RotateAndOpen(angleSetTime));
            }
            else
            {
                TryOpenFirstParachute();
            }

            openFirst = true;
        }
    }
    #endregion

    #region Try Open First Parachute
    public void TryOpenFirstParachute()
    {
        // initially move camera back
        animatorCamera.SetBool("MoveBack", true);

        // activate parts object
        rocketFull.gameObject.SetActive(false);
        rocketPart_All.gameObject.SetActive(true);

        // set proper materials
        SetRocketMaterial(materialSelected);

        // impulse to bottom and middletop part
        applyOnceFirst = true;

        // set gravity right away, kinematic after some time
        SetFirstDepartGravity(false);
        SetFirstObjectsKinematic();

        // rocket is departing, freeze all rotation for some time
        isDeparting = true;
        departTimer = 0f;
    }
    #endregion

    #region Open Second Parachute
    public void OpenSecondParachute()
    {
        if (openFirst == false || openSecond == true)
            return;

        // add components and open second parachute
        // give second objects rigidbodies, fixed joints
        rocketPart_Top.AddComponent<Rigidbody>();
        rocketPart_Top.AddComponent<FixedJoint>();
        sphere_FixedTop.AddComponent<Rigidbody>();
        sphere_FixedTop.AddComponent<FixedJoint>();

        rocketPart_Middle.AddComponent<Rigidbody>();
        rocketPart_Middle.AddComponent<FixedJoint>();

        // connect rope and objects
        rocketPart_Top.GetComponent<FixedJoint>().connectedBody = sphere_FixedTop.GetComponent<Rigidbody>();
        sphere_FixedTop.GetComponent<FixedJoint>().connectedBody = rocketPart_Top.GetComponent<Rigidbody>();
        secondRope[0].GetComponent<SpringJoint>().connectedBody = sphere_FixedTop.GetComponent<Rigidbody>();

        rocketPart_Middle.GetComponent<FixedJoint>().connectedBody = secondRope[secondRope.Length - 1].GetComponent<Rigidbody>();
        secondRope[secondRope.Length - 1].GetComponent<FixedJoint>().connectedBody = rocketPart_Middle.GetComponent<Rigidbody>();
        
        // middle rocket part is kinematic, rest of them can change
        rocketPart_Middle.GetComponent<Rigidbody>().isKinematic = true;
        SetSecondObjectsKinematic(true);

        // set kinematic to false and apply force to top part
        SetSecondDepartGravity(false);
        SetSecondObjectsKinematic(false);
        StartCoroutine(WaitAndSecondKinematic());

        applyOnceSecond = true;
        openSecond = true;
    }
    #endregion

    #region First Depart Kinematic
    public void SetFirstObjectsKinematic()
    {
        StartCoroutine(WaitAndFirstKinematic());
    }

    IEnumerator WaitAndFirstKinematic()
    {
        yield return new WaitForSeconds(setFirstKinematicTime / 5f);
        isMiddleTopMoving = true;
        isBottomMoving = true;
        yield return new WaitForSeconds(setFirstKinematicTime * 4f / 5f);

        // set object rb to kinematic and disable gravity
        rbMiddleTop.isKinematic = true;
        rbMiddleTopFixed.isKinematic = true;
        rbBottom.isKinematic = true;
        foreach(Rigidbody r in rbFirstRope)
        {
            r.isKinematic = true;
        }

        // open parachute here, works fine for now
        Instantiate(firstParachutePrefab, rbMiddleTopFixed.gameObject.transform.position, Quaternion.identity);

        // test, throw payload parachute here, impulse on payload parachute from middle bottom part
        applyOncePayloadParachute = true;
        // test, throw payload object here
        applyOncePayloadObject = true;

        // after set to kinematic, it's gonna rotate to horizontal pos
        // rotate departed rocket to proper position
        float rollAngle = transform.rotation.eulerAngles.x;
        float pitchAngle = transform.rotation.eulerAngles.z;
        rollAngle = (rollAngle >= 270f) ? (rollAngle - 360f) : rollAngle;
        pitchAngle = (pitchAngle >= 270f) ? (pitchAngle - 360f) : pitchAngle;

        if (Mathf.Abs(rollAngle) >= Mathf.Abs(pitchAngle))
        {
            departRollAngle = (rollAngle >= 0f) ? departingAngle : -departingAngle;
            departPitchAngle = pitchAngle;
        }
        else
        {
            departPitchAngle = (pitchAngle >= 0f) ? departingAngle : -departingAngle;
            departRollAngle = rollAngle;
        }
        RotateDepartedRocket(departRollAngle, departPitchAngle);
    }
    #endregion

    #region Second Depart Kinematic
    public void SetSecondObjectsKinematic(bool setValue)
    {
        rocketPart_Top.GetComponent<Rigidbody>().isKinematic = setValue;
        sphere_FixedTop.GetComponent<Rigidbody>().isKinematic = setValue;
        foreach (GameObject g in secondRope)
        {
            g.GetComponent<Rigidbody>().isKinematic = setValue; 
        }
    }

    IEnumerator WaitAndSecondKinematic()
    {
        yield return new WaitForSeconds(setSecondKinematicTime / 4f);
        isTopMoving = true;
        yield return new WaitForSeconds(setSecondKinematicTime * 3f / 4f);

        SetSecondObjectsKinematic(true);

        // open second parachute
        Instantiate(secondParachutePrefab, sphere_FixedTop.transform.position, Quaternion.identity);
    }
    #endregion

    #region First Depart Gravity
    public void SetFirstDepartGravity(bool isUsing)
    {
        rbMiddleTop.useGravity = isUsing;
        rbMiddleTopFixed.useGravity = isUsing;
        rbBottom.useGravity = isUsing;
        foreach (Rigidbody r in rbFirstRope)
        {
            r.useGravity = isUsing;
        }
    }
    #endregion

    #region Second Depart Gravity
    public void SetSecondDepartGravity(bool isUsing)
    {
        rocketPart_Top.GetComponent<Rigidbody>().useGravity = isUsing;
        rocketPart_Middle.GetComponent<Rigidbody>().useGravity = isUsing;
        sphere_FixedTop.GetComponent<Rigidbody>().useGravity = isUsing;
        foreach(GameObject g in secondRope)
        {
            g.GetComponent<Rigidbody>().useGravity = isUsing;
        }
    }
    #endregion

    // handling initia parachute open error
    // when rocket has low angles
    IEnumerator RotateAndOpen(float delay)
    {
        yield return new WaitForSeconds(delay);
        TryOpenFirstParachute();
    }
}
