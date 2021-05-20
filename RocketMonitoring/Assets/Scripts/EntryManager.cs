using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EntryManager : MonoBehaviour
{
    // static datas to input main scene elements
    public static string dataCOM = "";
    public static int dataBaudRate = 9600;
    public static float dataObtainPeriod = 500f;

    [SerializeField]
    TMP_Dropdown dropDown_Ports;

    [SerializeField] 
    TMP_Dropdown dropDown_BaudRates;

    [SerializeField]
    TMP_InputField inputField_Period;

    public void TrySwitchScene()
    {
        StartCoroutine(Darken_and_LoadScene());
    }

    IEnumerator Darken_and_LoadScene()
    {
        // animate, wait, load, animate
        SceneTransitions.instance.DarkenGame();
        yield return new WaitForSecondsRealtime(SceneTransitions.instance.animationTime);
        SceneLoader.Load(SceneType.Main);
        SceneTransitions.instance.LightenGame();
    }
}
