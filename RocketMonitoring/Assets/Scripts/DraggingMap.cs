using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public enum MissingPointerType
{
    RocketPointer,
    BasePointer,
    PayLoadPointer
}

public class DraggingMap : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;
    public static float dragX = 0f;
    public static float dragZ = 0f;
    public static bool isDragging = false;
    public static bool isMouseInRegion = false;

    [Header("Minimap Text Parameters")]
    [SerializeField]
    private TextMeshProUGUI textMetersRange;
    private float prevScale = 1f;
    private float currentScale = 1f;
    private bool scaleChanged = false;

    [Header("Minimap Pointer Parameters")]
    [SerializeField]
    private GameObject prefabRocketPointer;   // it will be base, rocket and later payload
    [SerializeField]
    private GameObject prefabBasePointer;
    [SerializeField]
    private GameObject prefabPayLoadPointer;
    [SerializeField]
    private RectTransform upLeftRT;
    [SerializeField]
    private RectTransform upRightRT;
    [SerializeField]
    private RectTransform downLeftRT;
    [SerializeField]
    private RectTransform downRightRT;
    [SerializeField]
    private List<RectTransform> cornerRTList;
    private GameObject rocketPointer;
    private GameObject basePointer;
    private GameObject payLoadPointer;

    // missing pointer conditions for rocket, base and payload
    // set pointeron check
    public static bool basePointerOn = false;
    public static bool rocketPointerOn = false;
    public static bool payloadPointerOn = false;
    // set direction and scale outside
    public static Vector2 baseOutsideDir, rocketOutsideDir, payloadOutsideDir;
    public static float baseOutsideScale, rocketOutsideScale, payloadOutsideScale;
    // active condition, on this class
    private bool basePointerActive = false;
    private bool rocketPointerActive = false;
    private bool payloadPointerActive = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        textMetersRange.text = "200 M RANGE";

        // corner points to a list
        cornerRTList.Add(upLeftRT);
        cornerRTList.Add(upRightRT);
        cornerRTList.Add(downLeftRT);
        cornerRTList.Add(downRightRT);

        // instantiate and deactivate pointer UI objects
        rocketPointer = Instantiate(prefabRocketPointer, gameObject.transform);
        basePointer = Instantiate(prefabBasePointer, gameObject.transform);
        payLoadPointer = Instantiate(prefabPayLoadPointer, gameObject.transform);
    }

    void Update()
    {
        // dragging code
        if(isDragging)
        {
            Vector2 localpoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out localpoint);

            Vector2 normalizedPoint = Rect.PointToNormalized(rectTransform.rect, localpoint);
            dragX = normalizedPoint.x;
            dragZ = normalizedPoint.y;
        }


        // check if scale changed, if it did, change range text
        prevScale = currentScale;
        currentScale = SpawnOnMapCustom.instance.currentScale;
        scaleChanged = (currentScale != prevScale) ? true : false;
        if(scaleChanged)
            AssignRangeText(currentScale);


        // Activate or Deactivate missing pointers, base, rocket and payload
        ShowHidePointers(MissingPointerType.RocketPointer);
        ShowHidePointers(MissingPointerType.BasePointer);
        ShowHidePointers(MissingPointerType.PayLoadPointer);

        // Move missing pointers on minimap
        if(basePointerOn && basePointerActive)
        {
            basePointer.GetComponent<MissingPointerControl>().MovePointer(cornerRTList, baseOutsideDir, baseOutsideScale);
        }
        if (rocketPointerOn && rocketPointerActive)
        {
            rocketPointer.GetComponent<MissingPointerControl>().MovePointer(cornerRTList, rocketOutsideDir, rocketOutsideScale);
        }
        if (payloadPointerOn && payloadPointerActive)
        {
            payLoadPointer.GetComponent<MissingPointerControl>().MovePointer(cornerRTList, payloadOutsideDir, payloadOutsideScale);
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        dragX = 0f;
        dragZ = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseInRegion = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseInRegion = false;
    }

    private void AssignRangeText(float scale)
    {
        string metersString = "";
        string unitsString = "";
        string remainingString = "RANGE";
        float metersRange = scale * 200f;
        
        if(metersRange < 1000)
        {
            metersString = Mathf.Round(metersRange).ToString();
            unitsString = " M";
        }
        else
        {
            metersString = (metersRange / 1000f).ToString();
            unitsString = " KM";
        }
        remainingString = " RANGE";
        textMetersRange.text = metersString + unitsString + remainingString;
    }

    // give missing pointer type, show or hide pointers due to bool values
    private void ShowHidePointers(MissingPointerType type)
    {
        switch(type)
        {
            case MissingPointerType.RocketPointer:

                if (rocketPointerOn && rocketPointerActive == false)
                {
                    rocketPointerActive = true;
                    rocketPointer.SetActive(true);
                }
                else if (rocketPointerOn == false && rocketPointerActive)
                {
                    rocketPointerActive = false;
                    rocketPointer.SetActive(false);
                }
                break;

            case MissingPointerType.BasePointer:

                if (basePointerOn && basePointerActive == false)
                {
                    basePointerActive = true;
                    basePointer.SetActive(true);
                }
                else if (basePointerOn == false && basePointerActive)
                {
                    basePointerActive = false;
                    basePointer.SetActive(false);
                }
                break;

            case MissingPointerType.PayLoadPointer:

                if (payloadPointerOn && payloadPointerActive == false)
                {
                    payloadPointerActive = true;
                    payLoadPointer.SetActive(true);
                }
                else if (payloadPointerOn == false && payloadPointerActive)
                {
                    payloadPointerActive = false;
                    payLoadPointer.SetActive(false);
                }
                break;
        }
    }
}
