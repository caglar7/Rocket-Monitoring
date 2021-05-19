using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public static LogManager instance;

    [Header("Menu Items")]
    [SerializeField] GameObject menuBox_Log;
    [SerializeField] GameObject menuBox_Info;

    [Header("Menu Title Parameters")]
    [SerializeField] Color color_DefaultTitleText;
    [SerializeField] Color color_SelectedTitleText;
    [SerializeField] Color color_DefaultTitleImage;
    [SerializeField] Color color_SelectedTitleImage;
    [SerializeField] Image titleBack_Log;
    [SerializeField] Image titleBack_Info;
    [SerializeField] Image titleText_Log;
    [SerializeField] Image titleText_Info;

    [Header("Log Menu Parameters")]
    [SerializeField] List<Message> messageList = new List<Message>();
    [SerializeField] int maxMessageAmount = 20;
    [SerializeField] GameObject chatPanel, textObject; 

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

        // active log menu, deactivate info menu
        menuBox_Log.gameObject.SetActive(true);
        menuBox_Info.gameObject.SetActive(false);
    }

    public void ShowInfo()
    {
        // activate info title
        titleBack_Info.color = color_SelectedTitleImage;
        titleText_Info.color = color_SelectedTitleText;
        // deactivate log title
        titleBack_Log.color = color_DefaultTitleImage;
        titleText_Log.color = color_DefaultTitleText;

        // active info menu, deactivate log menu
        menuBox_Info.gameObject.SetActive(true);
        menuBox_Log.gameObject.SetActive(false);
    }

    public void SendMessageToLog(string text)
    {
        if(messageList.Count >= maxMessageAmount)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.RemoveAt(0);
        }

        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;

        messageList.Add(newMessage);
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public Text textObject;
}
