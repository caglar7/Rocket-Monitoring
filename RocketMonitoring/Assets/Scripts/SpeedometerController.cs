using System.Collections;
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
    [SerializeField]
    private float speedSetTime = 0.5f;

    [Header("Text Display")]
    [SerializeField]
    TextMeshProUGUI textSpeed;

    float speedDiff = 0f;
    
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
    }

    public void SetSpeed(float s)
    {
        speedTarget = s;
        speedDiff = s - speed;
    }
}
