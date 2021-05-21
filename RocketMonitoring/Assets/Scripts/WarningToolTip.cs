using TMPro;
using UnityEngine;

public class WarningToolTip : MonoBehaviour
{
    RectTransform canvasRectTransform;
    RectTransform rectTransform;
    RectTransform rectBackground;
    TextMeshProUGUI textToolTip;

    // Start is called before the first frame update
    void Start()
    {
        canvasRectTransform = GameObject.FindWithTag("Canvas_EntryScene").GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
        foreach(Transform t in transform)
        {
            if (t.name == "background")
                rectBackground = t.GetComponent<RectTransform>();
            if (t.name == "text")
                textToolTip = t.GetComponent<TextMeshProUGUI>();
        }

        SetText(EntryManager.warningString);
    }

    void Update()
    {
        rectTransform.anchoredPosition = Input.mousePosition / canvasRectTransform.localScale.x;
    }

    public void SetText(string text)
    {
        textToolTip.text = text;
        textToolTip.ForceMeshUpdate();
        Vector2 textSize = textToolTip.GetRenderedValues(false);
        Vector2 paddingSize = new Vector2(10f, 20f);
        rectBackground.sizeDelta = textSize + paddingSize;
    }
}
