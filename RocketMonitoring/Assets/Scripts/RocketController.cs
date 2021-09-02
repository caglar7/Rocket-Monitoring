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

    // depart modeling
    private bool applyOnceFirst = false;
    private bool applyOnceSecond = false;

    private float departRollAngle = 0f;
    private float departPitchAngle = 0f;

    private bool isDeparting = false;
    private float departTimer;

    // rocket parts individual floating movement check
    public static bool isMiddleTopMoving = false;
    public static bool isBottomMoving = false;
    public static bool isTopMoving = false;

    [Header("TEST OPENING PARACHUTES")]
    [SerializeField]
    private bool openFirst = false;
    [SerializeField]
    private bool openSecond = false;
 
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
        if (openFirst)
        {
            openFirst = false;
            OpenFirstParachute();
            Debug.Log("opening 1");
        }

        if (openSecond)
        {
            openSecond = false;
            Debug.Log("opening 2");
            OpenSecondParachute();
        }

        if (applyOnceFirst)
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

    private void RotateDepartedRocket(float roll, float pitch)
    {
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
        Instantiate(firstParachutePrefab, rbMiddleTopFixed.position, Quaternion.identity);

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
}
