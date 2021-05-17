using UnityEngine;
using UnityEngine.EventSystems;

public class DraggingMap : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;
    public static float dragX = 0f;
    public static float dragZ = 0f;
    public static bool isDragging = false;
    public static bool isMouseInRegion = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if(isDragging)
        {
            Vector2 localpoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
                Input.mousePosition, GetComponentInParent<Canvas>().worldCamera, out localpoint);

            Vector2 normalizedPoint = Rect.PointToNormalized(rectTransform.rect, localpoint);
            dragX = normalizedPoint.x;
            dragZ = normalizedPoint.y;
        }

        Debug.Log("mouse in: " + isMouseInRegion);
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
}
