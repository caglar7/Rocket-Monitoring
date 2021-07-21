using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

// DISPLAYED DATA
// LAT LONG A V FRT SND LAT_B LONG_B
// FRT SND WILL TRIGGER ACTIONS ON THE ROCKET
// ANGLE IS A STATIC VARIABLE ON THE ROCKETCONTROLLER, directions

// DATA ORDER AND INDEXES, IN STRING ARRAY, A AND B ARE FOR VALID CHECK
/*
    A       0
    LAT     1
    LONG    2 
    A       3
    V       4
    AN      5
    FRT     6
    SND     7
    LAT_B   8
    LONG_B  9
    B       10
*/

public enum ErrorType
{
    Multiple,
    NoDataShort,
    NoDataLong,
    FullHalf,
    HalfFull,
    OnlyHalfB,
    OnlyHalfA,
    HalfHalf
}

public class DisplayData : MonoBehaviour
{
    // com and baud rate later will initially selected 
    SerialPort sp;

    private bool readAvailable = true;

    [Header("Velocity Parameters")]
    [SerializeField]
    SpeedometerController speedometer;

    [Header("Altitude Parameters")]
    [SerializeField]
    TextMeshProUGUI textAltitude;

    [Header("Parachute Displays")]
    [SerializeField]
    Image firstParachute;
    [SerializeField]
    Image secondParachute;
    [SerializeField]
    Color defaultColor;
    [SerializeField]
    Color greenColor;

    // CHECK CONDITIONS
    private int dataSize = 11;
    private bool dataFirstObtained = false;
    private bool firstHalfSecondValid = false;
    private bool firstValidSecondHalf = false;
    private bool dataUsable = false;

    // TIMER PARAMETERS
    // make sure this matches with arduino data transmission freq 
    private float readPeriodRemaining;
    private float readPeriod = 1f;
    // timer to check, no data time
    private float periodNoDataLong = 4f;
    private float timerNoData = 0f;
    private bool startNoDataTimer = false;

    void Start()
    {
        // assign data from EntryManager
        readPeriod = EntryManager.dataObtainPeriod;
        StopBits stopBits = StopBits.One;
        // set 0 8 stopbits later to test again
        sp = new SerialPort(EntryManager.dataCOM, EntryManager.dataBaudRate, 0, 8, stopBits);
        sp.ReadTimeout = 100;
        sp.Open();

        readPeriodRemaining = readPeriod;
    }

    void Update()
    {
        // TIMERS ---------------------------------------------------------------------------------
        // start measuring time when there is no data
        if(startNoDataTimer == true)
        {
            timerNoData += Time.deltaTime;
        }

        readPeriodRemaining -= Time.deltaTime;
        if (readPeriodRemaining <= 0f)
        {
            readPeriodRemaining = readPeriod;
            readAvailable = true;
        }

        if (readAvailable)
        {
            readAvailable = false;
        }
        else
            return;

        // -----------------------------------------------------------------------------------------

        if (sp.IsOpen)
        {
            try
            {
                // get data ----------------------------------------------------------------------
                string receivedData = "";
                string correctData = "";

                try
                {
                    receivedData = sp.ReadExisting();
                    sp.BaseStream.Flush();
                }
                catch (Exception e)
                {
                    LogManager.instance.SendMessageToLog(e.Message);
                }
                List<string> datasRaw = receivedData.Split(':').ToList();
                List<string> datas = new List<string>();

                // --------------------------------------------------------------------------------


                // valid check and extract proper data -----------------------------------------------
                // data format -> A....B 

                dataUsable = false;
                firstValidSecondHalf = false;
                firstHalfSecondValid = false;

                if (datasRaw.Count == dataSize && datasRaw[0] == "A")
                {
                    dataUsable = true;
                    datas = datasRaw.GetRange(1, dataSize - 2);
                }
                else
                {
                    // debug error type with received string, to check if profiler is working correct
                    ErrorCorrection(datasRaw);
                }

                // -----------------------------------------------------------------------------------

                // if data is valid, do something
                if (dataUsable)
                {
                    if (dataFirstObtained == false)
                    {
                        dataFirstObtained = true;
                        LogManager.instance.SendMessageToLog("First Data is obtained");
                    }

                    // altitude display, convert float then format to proper string
                    float altitudeData = float.Parse(datas[2]) / 100f;
                    textAltitude.text = altitudeData.ToString();

                    // velocity unit conversion and set on speedometer, 3
                    float speedData_meters = float.Parse(datas[3]) / 100f;
                    speedometer.SetSpeed(speedData_meters);

                    // assign rotation directions string on the RocketController.cs, 4
                    string[] RPstrings = datas[4].Split(',');
                    RocketController.instance.RotateRocket(RPstrings[0], RPstrings[1]);

                    // parachute displays, 5 and 6
                    // 1st
                    if (datas[5] == "1")
                        firstParachute.color = greenColor;
                    else if (datas[5] == "0")
                        firstParachute.color = defaultColor;
                    // 2nd
                    if (datas[6] == "1")
                        secondParachute.color = greenColor;
                    else if (datas[6] == "0")
                        secondParachute.color = defaultColor;

                    // pass lat long to the map script, base 7 8, rocket 0 1
                    SpawnOnMapCustom.instance.SetBasePosition(datas[7] + "," + datas[8]);
                    SpawnOnMapCustom.instance.SetRocketPosition(datas[0] + "," + datas[1]);

                    // when data is valid, print it on log
                    //LogManager.instance.SendMessageToLog(receivedData);
                }
                else
                {
                    Debug.Log("Data Error: " + receivedData);
                }
            }
            catch (System.Exception)
            {
                
            }
        }
    }

    void ErrorCorrection(List<String> rawDataList)
    {
        ErrorType type;
        
        
    }
}


/* TEXT DISPLAY CODE
 * 
    // display lat and long, 0 and 1
    textRocketLatLong.text = "" + "Rocket Coordinates\n";
    textRocketLatLong.text += datas[0] + "\n";
    textRocketLatLong.text += datas[1];

    // display altitude and velocity, 2 and 3
    textRocketAltitude.text = "" + "Altitude\n";
    textRocketAltitude.text += datas[2];
    textRocketVelocity.text = "" + "Velocity\n";
    textRocketVelocity.text += datas[3];

    // display first and second parachute, 5 and 6
    if (datas[5] == "0")
        textFirstParachute.color = colorDefaultFrtSnd;
    else
        textFirstParachute.color = colorGreenFrtSnd;

    if (datas[6] == "0")
        textSecondParachute.color = colorDefaultFrtSnd;
    else
        textSecondParachute.color = colorGreenFrtSnd;

    // display base lat and long, 7 and 8
    textBaseLatLong.text = "" + "Base Coordinates\n";
    textBaseLatLong.text += datas[7] + "\n";
    textBaseLatLong.text += datas[8];

 */