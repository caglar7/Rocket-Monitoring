using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ExitMenuButton
{
    Exit,
    Cancel
}

[RequireComponent(typeof(Button))]
public class Button_ExitMenu : MonoBehaviour
{
    Button button;

    [SerializeField]
    ExitMenuButton buttonType;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ExitMenuAction);
    }

    void ExitMenuAction()
    {
        switch(buttonType)
        {
            case ExitMenuButton.Exit:
                SceneTransitions.instance.ExitApplication();
                break;
            case ExitMenuButton.Cancel:
                SceneTransitions.instance.DeactivateExitMenu();
                break;
        }
    }
}