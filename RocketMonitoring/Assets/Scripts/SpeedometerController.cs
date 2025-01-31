﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedometerController : MonoBehaviour
{
    [Header("Pin Object")]
    [SerializeField]
    Transform pinTransform;

    [Header("Parameters")]
    [SerializeField]
    private float MIN_ANGLE = 0f;
    [SerializeField]
    private float MAX_ANGLE = 0f;
    [SerializeField]
    private float MAX_SPEED = 240f;
    [SerializeField]
    private float speed = 0f;
    [SerializeField]
    private float speedTarget = 0f;

    [Header("Text Display")]
    [SerializeField]
    TextMeshProUGUI textSpeed;

    float speedDiff = 0f;
    float speedSetTime = 1f;
    bool isPositive = true;

    void Start()
    {
        // get data from EntryManager
        speedSetTime = EntryManager.dataObtainPeriod;
    }

    void Update()
    {
        // check if speed reached the target
        float currentDiff = speedTarget - speed;
        if ((currentDiff * speedDiff) <= 0f)
            return;

        speed += (speedDiff * Time.deltaTime / speedSetTime);
        float speedNormalized = Mathf.Clamp(speed / MAX_SPEED, 0f, 1f);
        float angle = MIN_ANGLE + (MAX_ANGLE - MIN_ANGLE) * speedNormalized;
        pinTransform.eulerAngles = new Vector3(0f, 0f, angle);

        // update speed text
        if(isPositive)
            textSpeed.text = speed.ToString("0.0");
        else
            textSpeed.text = "-" + speed.ToString("0.0");
    }

    public void SetSpeed(float s)
    {
        if (s >= 0f)
            isPositive = true;
        else
            isPositive = false;

        speedTarget = Mathf.Abs(s);
        speedDiff = Mathf.Abs(s) - speed;
    }
}
