using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO.Ports;
using System;

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

    // check bools
    bool checkCOMPort;
    bool checkDataPeriod;

    // check data
    string[] ports;

    void Start()
    {

    }

    public void TrySwitchScene()
    {
        // CHECK INPUTS ------------------------------------------------------
        // ports
        checkCOMPort = false;
        int portIndex = dropDown_Ports.value;
        string portSelected = dropDown_Ports.options[portIndex].text;
        ports = SerialPort.GetPortNames();
        foreach (string port in ports)
        {
            if (portSelected == port)
            {
                checkCOMPort = true;
                break;
            }
        }
        // data period
        checkDataPeriod = true;
        string dataPeriod_Raw = inputField_Period.text;
        foreach (char c in dataPeriod_Raw)
        {
            if(!char.IsDigit(c))
            {
                checkDataPeriod = false;
                Debug.Log("Data period must be integer");
                break;
            }
        }
        if (dataPeriod_Raw == "")
        {
            checkDataPeriod = false;
            Debug.Log("Enter Data period");
        }
        // CHECKS ARE DONE --------------------------------------------------   END


        // CHECK CONDITIONS ----------------------------------------------------
        if(checkCOMPort == false || checkDataPeriod == false)
        {
            // show warning toolbar here
            return;
        }
        // -----------------------------------------------------------------    END


        // ASSIGN VALUES -------------------------------------------------------
        // port
        dataCOM = portSelected;
        // baud rate 
        int baudRate_Index = dropDown_BaudRates.value;
        string baudRate_Raw = dropDown_BaudRates.options[baudRate_Index].text;
        string baudRate_NoUnit = "";
        foreach(char c in baudRate_Raw)
        {
            if (char.IsDigit(c))
                baudRate_NoUnit += c;
        }
        dataBaudRate = Int32.Parse(baudRate_NoUnit);
        // data period
        dataObtainPeriod = float.Parse(dataPeriod_Raw);
        // -------------------------------------------------------------------  END 

        // START NEXT SCENE
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
