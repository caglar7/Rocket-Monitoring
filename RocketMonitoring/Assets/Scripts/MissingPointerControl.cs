using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// TEST THIS ON MANY DIFFERENT LOCATIONS
// CITIES DISTRICTS ETC.

public class MissingPointerControl : MonoBehaviour
{
    RectTransform mainRT;
    List<RectTransform> listRT;
    RectTransform arrowRT;
    RectTransform textRT;

    private float arrowWidth = 16.84f;
    private float arrowHeight = 27.84f;

    // test with timer
    float timerPeriod = 2f;
    float timer = 0f;

    void Start()
    {
        mainRT = GetComponent<RectTransform>();

        listRT = gameObject.GetComponentsInChildren<RectTransform>().ToList();
        for(int i=0; i<listRT.Count; i++)
        {
            if(listRT[i].gameObject.transform.name == "Arrow")
                arrowRT = listRT[i];

            if (listRT[i].gameObject.transform.name == "TextPointerName")
                textRT = listRT[i];
        }
    }

    // input will direction, pointertype
    public void MovePointer(List<RectTransform> rtList, Vector2 direction, float scale)
    {
        // check gameObject rect transform at first
        if (mainRT == null || direction == Vector2.zero)
        {
            Debug.Log("mainRT in MissingPointerControl() is null, or direction is zero");
            return;
        }

        // Get angle from pointer direction
        float angle = Mathf.Atan2(0f - direction.y, 0f - direction.x) * 180f / Mathf.PI;
        angle += 180f;
        angle = (angle >= 360f) ? (angle - 360f) : angle;

        // find 2 refs RT, rtList: upLeft, upRight, downLeft and downRight in order ------------
        RectTransform ref1, ref2;
        switch (angle)
        {
            // take downright and upright
            case 0f:
                ref1 = rtList[3];
                ref2 = rtList[1];
                break;
            // take upleft and upright
            case 90f:
                ref1 = rtList[0];
                ref2 = rtList[1];
                break;
            // take downleft and upleft
            case 180f:
                ref1 = rtList[2];
                ref2 = rtList[0];
                break;
            // take downleft and downright
            case 270f:
                ref1 = rtList[2];
                ref2 = rtList[3];
                break;
            default:
                ref1 = rtList[0];
                ref2 = rtList[1];
                break;
        }

        // -------------------------------------------------------------------------------------

        // Put UI pointer gameObject (this object) to assigned position
        Vector2 rtDiff = ref2.anchoredPosition - ref1.anchoredPosition;
        Vector2 pointerPos = ref1.anchoredPosition + (scale * rtDiff);
        mainRT.anchoredPosition = pointerPos;

        // Arrow Position and Rotation, Text Position ------------------------------------------
        arrowRT.rotation = Quaternion.Euler(0f, 0f, angle);
        float anchorX_Arrow = 0f;
        float anchorY_Arrow = 0f;
        float anchorX_Text = 0f;
        float anchorY_Text = 0f;

        if (angle == 180f || angle == 0f)
        {
            anchorX_Arrow = (angle == 0f) ? 1f : 0f;
            anchorY_Arrow = 0.5f;

            anchorX_Text = (angle == 0f) ? 0f : 1f;
            anchorY_Text = 0.5f;
        }
        else
        {
            anchorX_Arrow = 0.5f;
            anchorY_Arrow = (angle == 90f) ? 1f : 0f;

            anchorX_Text = 0.5f;
            anchorY_Text = (angle == 90f) ? 0f : 1f;

        }
        arrowRT.anchorMin = new Vector2(anchorX_Arrow, anchorY_Arrow);
        arrowRT.anchorMax = new Vector2(anchorX_Arrow, anchorY_Arrow);
        textRT.pivot = new Vector2(anchorX_Text, anchorY_Text);
        textRT.anchorMin = new Vector2(anchorX_Text, anchorY_Text);
        textRT.anchorMax = new Vector2(anchorX_Text, anchorY_Text);
        textRT.anchoredPosition = new Vector2(0f, 0f);
        // ----------------------------------------------------------------------------------
    }
}
