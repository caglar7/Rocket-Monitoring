using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Button_MaterialSelection : MonoBehaviour
{
    Button button;

    [Header("Button Parameters")]
    [SerializeField]
    private int buttonMaterialIndex;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SetMaterial);

        // activate or disable button with info from the rocket
        // ...
    }

    void SetMaterial()
    {
        RocketController.instance.SetRocketMaterial(buttonMaterialIndex);
    }
}
