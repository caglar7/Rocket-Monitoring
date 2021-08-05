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
    ID      1
    TIME    2
    LAT     3
    LONG    4 
    A       5
    V       6
    AN      7
    FRT     8
    SND     9
    LAT_B   10
    LONG_B  11
    B       12
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
    HalfHalf,
    UnDefined
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
    private int dataSize = 13;
    private bool dataFirstObtained = false;
    private bool isDataUsable = false;
    private string errorStorageString = "";
    private string usableDataString = "";

    // TIMER PARAMETERS
    // make sure this matches with arduino data transmission freq 
    private float readPeriodRemaining;
    private float readPeriod = 1f;
    // timer to check, no data time
    private float periodNoDataLong = 4f;
    private float timerNoData = 0f;
    private bool startNoDataTimer = false;

    // TEST DELETE LATER
    private int countValid = 0;
    private int countNotValid = 0;

    void Start()
    {
        // assign data from EntryManager
        readPeriod = EntryManager.dataObtainPeriod;
        readPeriodRemaining = readPeriod;

        InitSerialPort();
    }

    void Update()
    {
        // TIMERS ---------------------------------------------------------------------------------
        // start measuring time when there is no data
        if (startNoDataTimer == true)
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

                isDataUsable = false;

                if (datasRaw.Count == dataSize && datasRaw[0] == "A")
                {
                    isDataUsable = true;
                    datas = datasRaw.GetRange(1, dataSize - 2);

                    // reset error parameters and strings
                    timerNoData = 0f;
                    startNoDataTimer = false;
                    errorStorageString = "";
                    usableDataString = "";
                }
                else
                {
                    // profile and handle errors
                    ErrorType errorType = ProfileError(receivedData);

                    // write storage and usable data
                    HandleError(errorType, receivedData);

                    // split usable data and check
                    datas = usableDataString.Split(':').ToList();
                    if (datas.Count == dataSize && datas[0] == "A")
                    {
                        // remove A and B from datas list
                        datas.RemoveAt(datas.Count - 1);
                        datas.RemoveAt(0);
                        isDataUsable = true;
                        Debug.Log("Error is used properly");
                    }
                    else
                    {
                        countNotValid++;
                        Debug.Log("Not Valid: " + countNotValid);
                        //Debug.Log("Usable Data: " + usableDataString);
                    }
                }

                // -----------------------------------------------------------------------------------

                //if data is valid, do something -------------------------------------------------------
                if (isDataUsable)
                {
                    if (dataFirstObtained == false)
                    {
                        dataFirstObtained = true;
                        LogManager.instance.SendMessageToLog("First Data is obtained");
                    }

                    // altitude display, convert float then format to proper string
                    float altitudeData = float.Parse(datas[4]) / 100f;
                    textAltitude.text = altitudeData.ToString();

                    // velocity unit conversion and set on speedometer, 3
                    float speedData_meters = float.Parse(datas[5]) / 100f;
                    speedometer.SetSpeed(speedData_meters);

                    // assign rotation directions string on the RocketController.cs, 4
                    string[] RPstrings = datas[6].Split(',');
                    RocketController.instance.RotateRocket(RPstrings[0], RPstrings[1]);

                    // 1st parachute
                    if (datas[7] == "1")
                        firstParachute.color = greenColor;
                    else if (datas[7] == "0")
                        firstParachute.color = defaultColor;
                    // 2nd parachute
                    if (datas[8] == "1")
                        secondParachute.color = greenColor;
                    else if (datas[8] == "0")
                        secondParachute.color = defaultColor;

                    // pass lat long to the map script, base 7 8, rocket 0 1
                    SpawnOnMapCustom.instance.SetBasePosition(datas[9] + "," + datas[10]);
                    SpawnOnMapCustom.instance.SetRocketPosition(datas[2] + "," + datas[3]);
                }
                // -----------------------------------------------------------------------------------------

            }
            catch (Exception e)
            {
                LogManager.instance.SendMessageToLog(e.Message);
            }
        }
    }

    // return type of error there is
    ErrorType ProfileError(string rawDataString)
    {
        ErrorType type;

        // check no data conditions
        if (rawDataString.Length <= 2 || rawDataString == "")
        {
            if (startNoDataTimer == false)
                startNoDataTimer = true;

            if (timerNoData >= periodNoDataLong)
                type = ErrorType.NoDataLong;
            else
                type = ErrorType.NoDataShort;

            return type;
        }

        // check other conditions in ABA, BA, BA etc. format
        string errorProfile = "";
        for(int i=0; i< rawDataString.Length; i++)
        {
            if(rawDataString[i] == 'A' || rawDataString[i] == 'B')
            {
                errorProfile += rawDataString[i];
            }
        }
        if(errorProfile.Length >= 4)
            errorProfile = errorProfile.Substring(errorProfile.Length - 4);


        // consider BABA also, might occur again, test a lot
        switch (errorProfile)
        {
            case "ABAB":
                type = ErrorType.Multiple;
                break;
            case "ABA":
            case "BABA":
                type = ErrorType.FullHalf;
                break;
            case "BAB":
            case "BBAB":
                type = ErrorType.HalfFull;
                break;
            case "B":
                type = ErrorType.OnlyHalfB;
                break;
            case "A":
                type = ErrorType.OnlyHalfA;
                break;
            case "BA":
                type = ErrorType.HalfHalf;
                break;
            default:
                type = ErrorType.UnDefined;
                break;
        }
        return type;
    }

    // make use of errors to extract data
    private void HandleError(ErrorType type, string rawDataString)
    {
        usableDataString = "";
        int indexA1 = rawDataString.IndexOf('A');
        int indexA2 = rawDataString.LastIndexOf('A');
        int indexB1 = rawDataString.IndexOf('B');
        int indexB2 = rawDataString.LastIndexOf('B');

        Debug.Log("Error Storage: " + errorStorageString);

        switch (type)
        {
            // when there is no data, reset strings and init serial port
            case ErrorType.NoDataShort:
            case ErrorType.NoDataLong:
                errorStorageString = "";
                break;

            // take last valid string
            case ErrorType.Multiple:
            case ErrorType.HalfFull:
                errorStorageString = "";
                usableDataString += rawDataString.Substring(indexA2);
                break;

            // take first valid string, put remaining to storage
            case ErrorType.FullHalf:
                errorStorageString = "";
                usableDataString += rawDataString.Substring(indexA1, indexB1 - indexA1 + 1);
                errorStorageString += rawDataString.Substring(indexA2);
                break;

            // only halfA string, straight to the storage
            case ErrorType.OnlyHalfA:
                errorStorageString = "";
                errorStorageString += rawDataString;
                break;

            // add half B to the half A on storage, making a full valid string, hope so...
            case ErrorType.OnlyHalfB:
                usableDataString += errorStorageString + rawDataString;
                errorStorageString = "";
                break;

            case ErrorType.HalfHalf:
                usableDataString += errorStorageString + rawDataString.Substring(0, indexB1+1);
                errorStorageString = "";
                errorStorageString += rawDataString.Substring(indexA2);
                break;
        }

        Debug.Log("Usable String : " + usableDataString);
    }

    void InitSerialPort()
    {
        StopBits stopBits = StopBits.One;
        sp = new SerialPort(EntryManager.dataCOM, EntryManager.dataBaudRate, 0, 8, stopBits);
        sp.ReadTimeout = 100;
        sp.Open();
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