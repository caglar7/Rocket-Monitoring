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

    [SerializeField]
    [Header("Rocket Movement Elements")]
    float angularVelocity = 30;

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
       
    }

    public void RotateRocket(string rollString, string pitchString)
    {
        float rollFloat = float.Parse(rollString) / 100f;
        float pitchFloat = float.Parse(pitchString) / 100f;
        transform.Rotate(rollFloat, 0f, pitchFloat);

    }
}
