using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public static LogManager instance;

    [Header("Menu Title Parameters")]
    [SerializeField] Color color_DefaultTitleText;
    [SerializeField] Color color_SelectedTitleText;
    [SerializeField] Color color_DefaultTitleImage;
    [SerializeField] Color color_SelectedTitleImage;
    [SerializeField] Image titleBack_Log;
    [SerializeField] Image titleBack_Info;
    [SerializeField] Image titleText_Log;
    [SerializeField] Image titleText_Info;

    void Start()
    {
        ShowLog();
    }

    public void ShowLog()
    {
        // activate log title
        titleBack_Log.color = color_SelectedTitleImage;
        titleText_Log.color = color_SelectedTitleText;
        // deactivate info title
        titleBack_Info.color = color_DefaultTitleImage;
        titleText_Info.color = color_DefaultTitleText;
    }

    public void ShowInfo()
    {
        // activate info title
        titleBack_Info.color = color_SelectedTitleImage;
        titleText_Info.color = color_SelectedTitleText;
        // deactivate log title
        titleBack_Log.color = color_DefaultTitleImage;
        titleText_Log.color = color_DefaultTitleText;
    }
}
