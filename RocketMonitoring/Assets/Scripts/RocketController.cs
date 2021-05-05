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
    public static string angleDirections = "000";

    [SerializeField]
    [Header("Rocket Movement Elements")]
    float angularVelocity = 30;

    void Start()
    {
        angleDirections = "000";
    }

    
    void Update()
    {
        // 0 static, 1 negative, 2 positive rotational direction, from static variable

        // string to int
        int xData = Int32.Parse(angleDirections[0].ToString());
        int yData = Int32.Parse(angleDirections[1].ToString());
        int zData = Int32.Parse(angleDirections[2].ToString());

        // convert to proper directions
        int xDir = (xData * 2) - 3;
        if (xData == 0)
            xDir = 0;

        int yDir = (yData * 2) - 3;
        if (yData == 0)
            yDir = 0;

        int zDir = (zData * 2) - 3;
        if (zData == 0)
            zDir = 0;

        float incrementX = xDir * angularVelocity * Time.deltaTime;
        float incrementY = yDir * angularVelocity * Time.deltaTime;
        float incrementZ = zDir * angularVelocity * Time.deltaTime;
        transform.Rotate(incrementX, incrementY, incrementZ);
    }
}
