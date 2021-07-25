using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        textMetersRange.text = "200 M RANGE";
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
        {
            AssignRangeText(currentScale);
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
}
