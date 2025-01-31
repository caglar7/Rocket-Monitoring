﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.IO.Ports;
using UnityEngine.UI;
using TMPro;
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
    LAT     3.
    LONG    4 .
    A       5.
    V       6.
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
    [SerializeField]
    SpeedometerController speedometerPayload;

    [Header("Altitude Parameters")]
    [SerializeField]
    TextMeshProUGUI textAltitude;
    [SerializeField]
    TextMeshProUGUI textAltitudePayload;

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
    private bool deactivateBooster = false;

    // TIMER PARAMETERS
    // make sure this matches with arduino data transmission freq 
    private float readPeriodRemaining;
    private float readPeriod = 1f;
    // timer to check, no data time
    private float periodNoDataLong = 4f;
    private float timerNoData = 0f;
    private bool startNoDataTimer = false;

    // Counting error data
    private int countValid = 0;
    private int countNotValid = 0;

    // FLIGHT RECORD PARAMETERS
    private string keyFlightRecordNumber = "keyFlightRecordCount";
    private int flightRecordCount;
    private string recordName = "";
    private string recordFileName  = "";
    private string recordNamePayload = "";
    private string recordFileNamePayload = "";
    private TextWriter textWriter;
    private TextWriter textWriterPayload;
    private DateTime localDate;
    private Char char1;
    private Char char2;

    void InitSerialPort()
    {
        StopBits stopBits = StopBits.One;
        sp = new SerialPort(EntryManager.dataCOM, EntryManager.dataBaudRate, 0, 8, stopBits);
        sp.ReadTimeout = 100;
        sp.Open();
    }

    void Start()
    {
        // commma or dot in order for excel, for libre and mic
        char1 = EntryManager.excelCommaOrDot[0];
        char2 = EntryManager.excelCommaOrDot[1];

        // get data from player pref about flight records
        flightRecordCount = PlayerPrefs.GetInt(keyFlightRecordNumber, 0);
        recordName = "\\FlightRecord_" + flightRecordCount.ToString() + ".csv";
        recordFileName = EntryManager.flightRecordsPath + recordName;

        // paylaod csv file
        recordNamePayload = "\\FlightRecordPayload_" + flightRecordCount.ToString() + ".csv";
        recordFileNamePayload = EntryManager.flightRecordsPath + recordNamePayload;

        PlayerPrefs.SetInt(keyFlightRecordNumber, flightRecordCount + 1);
        textWriter = new StreamWriter(recordFileName, false);
        textWriter.WriteLine("LOCAL TIME" + ";" + "ID" + ";" + "TIME" + ";" + "ROCKET_LAT" + ";" + "ROCKET_LONG"
            + ";" + "ALTITUDE" + ";" + "VELOCITY" + ";" + "ROLL" + ";" + "PITCH" + ";" + "FIRST"
            + ";" + "SECOND" + ";" + "BASE_LAT" + ";" + "BASE_LONG");
        textWriter.Close();

        // writing to payload csv file
        textWriterPayload = new StreamWriter(recordFileNamePayload, false);
        textWriterPayload.WriteLine("LOCAL TIME" + ";" + "ID" + ";" + "TIME" + ";" + "PAYLOAD_LAT" + ";" + "PAYLOAD_LONG"
            + ";" + "ALTITUDE" + ";" + "VELOCITY");
        textWriterPayload.Close();

        // assign data from EntryManager
        readPeriod = EntryManager.dataObtainPeriod;
        readPeriodRemaining = readPeriod;

        StopBits stopBits = StopBits.One;
        sp = new SerialPort(EntryManager.dataCOM, EntryManager.dataBaudRate, 0, 8, stopBits);
        sp.ReadTimeout = 100;
        sp.Open();

        Debug.Log("com: " + EntryManager.dataCOM);
        Debug.Log("baud:" + EntryManager.dataBaudRate);
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

                    // check if id 0 or 1
                    if (datas[0] == "0")
                        ModelRocket(datas);

                    else if (datas[0] == "1")
                        ModelPayLoad(datas);

                }
                // -----------------------------------------------------------------------------------------

                // print data to log screen
                LogManager.instance.SendMessageToLog("DATA: " + receivedData);
            }
            catch (Exception e)
            {
                LogManager.instance.SendMessageToLog(e.Message);
            }
        }
    }

    #region Error Handling
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
    }
    #endregion

    private void ModelRocket(List<string> datas)
    {
        Debug.Log("first: " + datas[7] + "second: " + datas[8]);

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
        {
            firstParachute.color = greenColor;
            if(deactivateBooster == false)
            {
                deactivateBooster = true;
                if (RocketController.instance.boosterEffect != null)
                    RocketController.instance.boosterEffect.Stop();
            }
            RocketController.instance.OpenFirstParachute();
        }
        else if (datas[7] == "0")
            firstParachute.color = defaultColor;

        // 2nd parachute
        if (datas[8] == "1")
        {
            secondParachute.color = greenColor;
            RocketController.instance.OpenSecondParachute();
        }
        else if (datas[8] == "0")
            secondParachute.color = defaultColor;

        // pass lat long to the map script, base 7 8, rocket 0 1
        SpawnOnMapCustom.instance.SetBasePosition(datas[9] + "," + datas[10]);
        SpawnOnMapCustom.instance.SetRocketPosition(datas[2] + "," + datas[3]);

        // SAVE DATA TO EXCEL ---------------------------------------------------------------
        string excelString = "";
        // date, id and time
        localDate = DateTime.Now;
        excelString += localDate.Hour + ":" + localDate.Minute + ":" + localDate.Second + ";";
        for (int i = 0; i < dataSize - 2; i++)
        {
            // check for roll and pitch
            if (i == 6)
            {
                excelString += RPstrings[0].Replace(char1, char2) + ";";
                excelString += RPstrings[1].Replace(char1, char2) + ";";
            }
            else
                excelString += datas[i].Replace(char1, char2) + ";";
        }

        textWriter = new StreamWriter(recordFileName, true);
        textWriter.WriteLine(excelString);
        textWriter.Close();

        // ------------------------------------------------------------------------------------
    }

    private void ModelPayLoad(List<string> datas)
    {
        // assign payload positions on minimap
        SpawnOnMapCustom.instance.SetPayLoadPosition(datas[2] + "," + datas[3]);

        // assign payload altitude
        float altitudeData = float.Parse(datas[4]) / 100f;
        textAltitudePayload.text = altitudeData.ToString();
        textAltitudePayload.text = altitudeData.ToString();

        // assign payload speed
        float speedData_meters = float.Parse(datas[5]) / 100f;
        speedometerPayload.SetSpeed(speedData_meters);

        // SAVE DATA TO EXCEL -----------------------------------------------------------------
        string excelString = "";
        // date
        localDate = DateTime.Now;
        excelString += localDate.Hour + ":" + localDate.Minute + ":" + localDate.Second + ";";
        // writing id, time, lat, long, altitude, speed, till index 5
        for(int i=0; i<6; i++)
        {
            excelString += datas[i].Replace(char1, char2) + ";";
        }
        textWriterPayload = new StreamWriter(recordFileNamePayload, true);
        textWriterPayload.WriteLine(excelString);
        textWriterPayload.Close();
        // ------------------------------------------------------------------------------------
    }
}