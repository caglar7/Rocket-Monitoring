using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO.Ports;
using System;
using System.Linq;
using SimpleFileBrowser;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using System.Globalization;

// TO RESET EVERY SAVED DATA OR MAP DATA
// GO MAPBOX>SETUP>CLEAR FILE CACHE ,  later will be from code
// THEN DELETE PLAYERPREFS


public class EntryManager : MonoBehaviour
{
    // static datas to input main scene elements
    public static string dataCOM = "";
    public static int dataBaudRate = 9600;
    public static float dataObtainPeriod = 500f;
    public static string warningString;
    public static string flightRecordsPath = "";
    public static bool isMouseOverPathText = false;

    [SerializeField]
    TMP_Dropdown dropDown_Ports;

    [SerializeField] 
    TMP_Dropdown dropDown_BaudRates;

    [SerializeField]
    TMP_InputField inputField_Period;

    [SerializeField]
    GameObject warningToolTip;

    [SerializeField]
    TextMeshProUGUI pathText;

    [SerializeField]
    Canvas canvas;

    [Header("Menu Objects")]
    [SerializeField]
    private GameObject mainMenuObject;

    [SerializeField]
    private GameObject offlineMapsMenuObject;

    // menu animators and bool parameters
    private Animator animatorMainMenu;
    private Animator animatorOfflineMapsMenu;
    private bool isMainActive = true;
    private bool isOfflineActive = false;
    // just a little longer than real animation time
    [SerializeField] float animationTime = 0.35f;

    // check bools
    private bool checkCOMPort;
    private bool checkDataPeriod;
    private bool checkFlightRecordPath;

    // entry parameters
    private List<string> ports;
    private List<string> checkPorts;
    private float periodPorts = 1f;
    private float timerPorts = 0f;

    // flight record, tooltip, playerprefs key
    private GameObject pathToolTip;
    private bool isPathTipActive = false;
    private string keyFlightRecordPath = "keyFlightRecords";

    [Header("Offline Maps Parameters")]
    [SerializeField]
    private TileCacher tileCacher;

    [SerializeField]
    private TMP_InputField inputField_TopLeft;

    [SerializeField]
    private TMP_InputField inputField_BottomRight;

    public static bool isDownloading = false;
    public static bool isDownloadFinished = false;
    public static int downloadedTiles = 0;
    public static string keyDownloadedTiles = "keyDownloadedTiles";
    private const int TILE_LIMIT = 3000;

    void Start()
    {
        // get downloaded tile amoung, later this will be the accurate cached tile amount
        downloadedTiles = PlayerPrefs.GetInt(keyDownloadedTiles, 0);
        tileCacher.UpdateTileLimitText(downloadedTiles);

        // get menu animators
        animatorMainMenu = mainMenuObject.GetComponent<Animator>();
        animatorOfflineMapsMenu = offlineMapsMenuObject.GetComponent<Animator>();
        offlineMapsMenuObject.SetActive(false);

        // update drop down ports
        UpdatePorts();

        dropDown_BaudRates.value = 9;
        inputField_Period.text = "500";

        // get previous flight record from playerprefs
        flightRecordsPath = PlayerPrefs.GetString(keyFlightRecordPath);
        pathText.text = " " + flightRecordsPath;
    }

    void Update()
    {
        // check ports, if there is a change update it
        timerPorts += Time.deltaTime;
        if(timerPorts >= periodPorts)
        {
            timerPorts = 0f;
            checkPorts = SerialPort.GetPortNames().ToList();
            if (checkPorts != ports)
                UpdatePorts();
        }

        // check if tiles are downloaded and close offlinemaps menu
        if (isDownloadFinished)
        {
            isDownloadFinished = false;
            ActivateMainMenu();
        }

        if(isMouseOverPathText && flightRecordsPath != "")
        {
            if(isPathTipActive == false)
            {
                warningString = " " + flightRecordsPath;
                isPathTipActive = true;
                pathToolTip = Instantiate(warningToolTip, canvas.transform);
            }
        }
        else
        {
            if(pathToolTip != null)
                pathToolTip.Destroy();
            isPathTipActive = false;
        }
            

        if(pathText.text != " " + flightRecordsPath)
        {
            pathText.text = " " + flightRecordsPath;
            // save flight records path
            PlayerPrefs.SetString(keyFlightRecordPath, flightRecordsPath);
        }
    }

    public void TrySwitchScene()
    {  
        // CHECK INPUTS ------------------------------------------------------
        warningString = "";
        // ports
        checkCOMPort = false;
        int portIndex = dropDown_Ports.value;
        string portSelected = dropDown_Ports.options[portIndex].text;
        foreach (string port in ports)
        {
            if (portSelected == port)
            {
                checkCOMPort = true;
                break;
            }
        }
        if (checkCOMPort == false)
            warningString += "- COM Port does not match!\n";
        // data period
        checkDataPeriod = true;
        string dataPeriod_Raw = inputField_Period.text;
        foreach (char c in dataPeriod_Raw)
        {
            if(!char.IsDigit(c))
            {
                checkDataPeriod = false;
                warningString += "- Data Period must be integer!\n";
                break;
            }
        }
        if (dataPeriod_Raw == "")
        {
            checkDataPeriod = false;
            warningString += "- Enter Data Period!\n";
        }
        // flight record path directory check
        checkFlightRecordPath = true;
        if (flightRecordsPath == "")
        {
            checkFlightRecordPath = false;
            warningString += "- Select Flight Record Path!\n";
        }
        // CHECKS ARE DONE --------------------------------------------------   END

        // CHECK CONDITIONS ----------------------------------------------------
        if (!checkCOMPort || !checkDataPeriod || !checkFlightRecordPath)
        {
            // show warning toolbar here, get rid of debugs and put a menu item
            GameObject toolTip = Instantiate(warningToolTip, canvas.transform);
            Destroy(toolTip, 3f);
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
        dataObtainPeriod = float.Parse(dataPeriod_Raw) / 1000f;
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

    public void OpenExplorer()
    {
        // open and assign file path here
        FileBrowser.ShowLoadDialog((paths) => flightRecordsPath = paths[0], null,           
                    FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select");

    }


    #region Menu Switching Methods
    public void ActivateMainMenu()
    {
        // make sure to call only once on initial button click
        if(!isMainActive)
        {
            offlineMapsMenuObject.GetComponent<CanvasGroup>().interactable = false;
            animatorOfflineMapsMenu.SetBool("ActivateMenu", false);
            animatorOfflineMapsMenu.SetBool("DeactivateMenu", true);

            animatorMainMenu.SetBool("ActivateMenu", true);
            animatorMainMenu.SetBool("DeactivateMenu", false);

            StartCoroutine(WaitActivateMainMenu());
        }
    }

    IEnumerator WaitActivateMainMenu()
    {
        yield return new WaitForSeconds(animationTime);
        tileCacher.ResetProgressBar();

        offlineMapsMenuObject.SetActive(false);
        mainMenuObject.GetComponent<CanvasGroup>().interactable = true;

        // when code is done
        isMainActive = true;
        isOfflineActive = false;
    }

    public void ActivateOfflineMapsMenu()
    {
        if(!isOfflineActive)
        {
            offlineMapsMenuObject.SetActive(true);

            offlineMapsMenuObject.GetComponent<CanvasGroup>().interactable = false;
            animatorOfflineMapsMenu.SetBool("ActivateMenu", true);
            animatorOfflineMapsMenu.SetBool("DeactivateMenu", false);

            mainMenuObject.GetComponent<CanvasGroup>().interactable = false;
            animatorMainMenu.SetBool("ActivateMenu", false);
            animatorMainMenu.SetBool("DeactivateMenu", true);

            StartCoroutine(WaitActivateOfflineMenu());
        }
    }

    IEnumerator WaitActivateOfflineMenu()
    {
        yield return new WaitForSeconds(animationTime);
        offlineMapsMenuObject.GetComponent<CanvasGroup>().interactable = true;
        tileCacher.UpdateTileLimitText(downloadedTiles);

        inputField_TopLeft.text = "";
        inputField_BottomRight.text = "";
        inputField_TopLeft.resetOnDeActivation = true;
        inputField_BottomRight.resetOnDeActivation = true;

        // when done
        isOfflineActive = true;
        isMainActive = false;
    }
    #endregion


    public void DownloadTiles()
    {
        // later add token exception handling and entering tokens in application
        // make a proper string check later
        warningString = "";
        bool checkString1 = false;
        bool checkString2 = false;
        bool checkConnection = false;
        bool checkTileLimit = true;

        string rawPointTopLeft = inputField_TopLeft.text;
        string rawPointBottomRight = inputField_BottomRight.text;

        string pointTopLeft = rawPointTopLeft.Replace(" ", "");
        string pointBottomRight = rawPointBottomRight.Replace(" ", "");

        // check input fields
        checkString1 = CheckLatLongString(pointTopLeft);
        checkString2 = CheckLatLongString(pointBottomRight);

        // check network connection
        if (Application.internetReachability != NetworkReachability.NotReachable)
            checkConnection = true;

        // check tile counts
        int tileCountToDownload = 0;
        if (checkString1 && checkString2)
        {
            tileCountToDownload = tileCacher.GetTileCount(17, pointTopLeft, pointBottomRight);
            if ((downloadedTiles + tileCountToDownload) <= TILE_LIMIT)
                checkTileLimit = true;
            else
                checkTileLimit = false;
        }

        // add warning message and return at this point
        // first check should have different method to handle, not conversion get type stuff
        if(!checkString1 || !checkString2)
            warningString += "- Check Input Fields (lat / long)!\n";
        if(!checkConnection)
            warningString += "- Check Network Connection!\n";
        if(!checkTileLimit)
            warningString += "- Map is too large to download (" + (downloadedTiles + tileCountToDownload).ToString() + "/" + TILE_LIMIT + ")\n";

        if(!checkString1 || !checkString2 || !checkConnection || !checkTileLimit)
        {
            GameObject toolTip = Instantiate(warningToolTip, canvas.transform);
            Destroy(toolTip, 3f);
            return;
        }

        if (!isDownloading)
        {
            isDownloading = true;
            tileCacher.CacheTiles(17, pointTopLeft, pointBottomRight);
        }
    }

    private bool CheckLatLongString(string s)
    {
        bool boolResult;

        foreach(char c in s)
        {
            if (Char.IsDigit(c) || c == ',' || c == '.')
            {
                // don't do anything
            }
            else 
                return false;
        }
        try
        {
            var latLonSplit = s.Split(',');
            double latitude = 0;
            double longitude = 0;

            if (!double.TryParse(latLonSplit[0], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out latitude))
            {
                Debug.LogError(string.Format("Could not convert latitude to double: {0}", latLonSplit[0]));
            }

            if (!double.TryParse(latLonSplit[1], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out longitude))
            {
                Debug.LogError(string.Format("Could not convert longitude to double: {0}", latLonSplit[0]));
            }
            Vector2d latlongVector = new Vector2d(latitude, longitude);
            boolResult = true;

        }
        catch
        {
            boolResult = false;
        }
        return boolResult;
    }

    private void UpdatePorts()
    {
        ports = SerialPort.GetPortNames().ToList();
        dropDown_Ports.ClearOptions();
        dropDown_Ports.AddOptions(ports);
    }
}
