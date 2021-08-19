using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Button_TileCacher : MonoBehaviour
{
    Button button;

    [SerializeField] TileCacher tileCacher;
    [SerializeField] string pointTopLeft;
    [SerializeField] string pointBottomRight;
    [SerializeField] int zoomLevel;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(StartCache);
    }

    private void StartCache()
    {
        tileCacher.CacheTiles(zoomLevel, pointTopLeft, pointBottomRight);
    }
}
