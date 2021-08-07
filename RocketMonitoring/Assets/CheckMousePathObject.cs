using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CheckMousePathObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        EntryManager.isMouseOverPathText = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EntryManager.isMouseOverPathText = false;
    }
}
