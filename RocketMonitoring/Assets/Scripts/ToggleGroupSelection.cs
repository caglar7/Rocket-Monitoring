using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ToggleGroupSelection : MonoBehaviour
{
    private ToggleGroup toggleGroup;

    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
        Debug.Log("Selected one: " + CurrentSelection.name);
    }


    public Toggle CurrentSelection
    {
        get 
        {
            return toggleGroup.ActiveToggles().FirstOrDefault();
        }
    }
}
