using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map.Interfaces;
using Mapbox.Unity.Map;
using Mapbox.Platform.Cache;
using TMPro;
using Mapbox.Unity;

public class TileCacher : MonoBehaviour
{
    public enum Status
    {
        ALL_CACHED,
        SOME_CACHED,
        ALL_FALIED,
        NOTHING_TO_CAHCE
    }
    public delegate void TileCacherEvent(Status result, int FetchedTileCount);

    [Header("Warning UI Object")]
    [SerializeField] GameObject _warningToolTip;

    [Header("Tile Object")]
    [SerializeField] GameObject tilePlaneObject;

    [Header("Cached Tile / Limit Text")]
    [SerializeField]
    private TextMeshProUGUI textTilesLimit;

    [Header("Area Data")]
    public List<string> Points;
    public string ImageMapId;
    public int ZoomLevel;

    [Header("Output")]
    public float Progress = 0f;
    private float prevProgress = 0f;
    [SerializeField] private float progressTimeOutPeriod = 10f;
    private float timeoutTimer = 0f;
    private bool isTimerOn = false;

    [TextArea(10, 20)]
    public string Log;
    private ImageDataFetcher ImageFetcher;
    private int _tileCountToFetch;
    private int _failedTileCount;
    [SerializeField] private int _currentProgress;
    private Vector2 _anchor;
    [SerializeField] private Transform _canvas;
    [SerializeField] bool DoesLog = false;
    [SerializeField] bool DoesRender = false;
    [SerializeField] Image progressBarImage;
    [SerializeField] TextMeshProUGUI progressBarText;
    public event TileCacherEvent OnTileCachingEnd;

    // edited parameters
    [SerializeField]
    private AbstractMap _map;

    private void Start()
    {
        // download progress parameters
        timeoutTimer = progressTimeOutPeriod;

        // ImageFetcher = new ImageDataFetcher();
        ImageFetcher = ScriptableObject.CreateInstance<ImageDataFetcher>();
        ImageFetcher.DataRecieved += ImageDataReceived;
        ImageFetcher.FetchingError += ImageDataError; 
    }

    void Update()
    {
        if(EntryManager.isDownloading)
        {
            if (Progress != prevProgress || Progress == 0f)
            {
                prevProgress = Progress;
                isTimerOn = false;
                timeoutTimer = progressTimeOutPeriod;
            }
            else
            {
                isTimerOn = true;
            }

            if (isTimerOn == true)
            {
                timeoutTimer -= Time.deltaTime;
                if (timeoutTimer <= 0f)
                {
                    // download failed, guide user
                    EntryManager.isDownloading = false;
                    EntryManager.isDownloadFinished = true;
                    CheckEnd();

                    // tooptip
                    EntryManager.warningString = "";
                    EntryManager.warningString += "- Download Failed, Try Again!\n";
                    GameObject toolTip = Instantiate(_warningToolTip, _canvas.transform);
                    Destroy(toolTip, 5f);
                }
                Debug.Log("TimeOut timer: " + timeoutTimer);
            }
        }
    }

    public void CacheTiles(int _zoomLevel, string _topLeft, string _bottomRight)
    {
        ZoomLevel = _zoomLevel;
        Points = new List<string>();
        Points.Add(_topLeft);
        Points.Add(_bottomRight);
        PullTiles();
    }

    [ContextMenu("Download Tiles")]
    public void PullTiles()
    {
        Progress = 0;
        prevProgress = 0;
        _tileCountToFetch = 0;
        _currentProgress = 0;
        _failedTileCount = 0;

        var pointMeters = new List<UnwrappedTileId>();
        foreach (var point in Points)
        {
            var pointVector = Conversions.StringToLatLon(point);
            var pointMeter = Conversions.LatitudeLongitudeToTileId(pointVector.x, pointVector.y, ZoomLevel);
            pointMeters.Add(pointMeter);
        }

        var minx = int.MaxValue;
        var maxx = int.MinValue;
        var miny = int.MaxValue;
        var maxy = int.MinValue;

        foreach (var meter in pointMeters)
        {
            if (meter.X < minx)
            {
                minx = meter.X;
            }

            if (meter.X > maxx)
            {
                maxx = meter.X;
            }

            if (meter.Y < miny)
            {
                miny = meter.Y;
            }

            if (meter.Y > maxy)
            {
                maxy = meter.Y;
            }
        }

        // If there is only one tile to fetch, this makes sure you fetch it
        if (maxx == minx)
        {
            maxx++;
            minx--;
        }

        if (maxy == miny)
        {
            maxy++;
            miny--;
        }

        _tileCountToFetch = (maxx - minx) * (maxy - miny);

        if (_tileCountToFetch == 0)
        {
            OnTileCachingEnd.Invoke(Status.NOTHING_TO_CAHCE, 0);
        }
        else
        {
            _anchor = new Vector2((maxx + minx) / 2, (maxy + miny) / 2);
            PrintLog(string.Format("{0}, {1}, {2}, {3}", minx, maxx, miny, maxy));
            StartCoroutine(StartPulling(minx, maxx, miny, maxy));
        }
    }

    private IEnumerator StartPulling(int minx, int maxx, int miny, int maxy)
    {

        for (int i = minx; i < maxx; i++)
        {
            for (int j = miny; j < maxy; j++)
            {

                ImageFetcher.FetchData(new ImageDataFetcherParameters()
                {
                    canonicalTileId = new CanonicalTileId(ZoomLevel, i, j),
                    tilesetId = ImageMapId,
                    tile = null
                });

                yield return null;
            }
        }
    }

    #region Fetcher Events

    private void ImageDataError(UnityTile arg1, RasterTile arg2, TileErrorEventArgs arg3)
    {
        PrintLog(string.Format("Image data fetching failed for {0}\r\n", arg2.Id));
        _failedTileCount++;
    }

    private void ImageDataReceived(UnityTile arg1, RasterTile arg2)
    {
        _currentProgress++;
        Progress = (float)_currentProgress / _tileCountToFetch * 100;
        if (progressBarImage != null && progressBarImage.gameObject.activeInHierarchy)
        {
            progressBarImage.fillAmount = Progress / 100;
            progressBarText.text = "Progress " + ((int)Progress).ToString() + " %";
        }
        RenderImagery(arg2);

        if (Progress == 100)
        {
            EntryManager.isDownloading = false;
            EntryManager.isDownloadFinished = true;
            CheckEnd();
        }
    }
    #endregion

    #region Utility Functions
    private void CheckEnd()
    {
        if (OnTileCachingEnd != null)
        {
            if (_failedTileCount == 0)
            {
                OnTileCachingEnd.Invoke(Status.ALL_CACHED, _tileCountToFetch);
            }
            else if (_failedTileCount == _tileCountToFetch)
            {
                OnTileCachingEnd.Invoke(Status.ALL_FALIED, 0);
            }
            else if (_failedTileCount > 0 && _failedTileCount < _tileCountToFetch)
            {
                OnTileCachingEnd.Invoke(Status.SOME_CACHED, _tileCountToFetch - _failedTileCount);
            }
        }

        // this should be deleted later if cached tiles amount is obtained
        EntryManager.downloadedTiles += _tileCountToFetch - _failedTileCount;
        PlayerPrefs.SetInt(EntryManager.keyDownloadedTiles, EntryManager.downloadedTiles);
        UpdateTileLimitText(EntryManager.downloadedTiles);
    }

    private void RenderImagery(RasterTile rasterTile)
    {
        if (!DoesRender || _canvas == null || !_canvas.gameObject.activeInHierarchy) return;

        // put raster tile image on mesh here on correct position
        GameObject tileMesh = Instantiate(tilePlaneObject, gameObject.transform);
        tileMesh.GetComponent<TileObject>().SetTileTexture(rasterTile.Data);
        Vector2d centerLatLong = Conversions.TileIdToCenterLatitudeLongitude(rasterTile.Id.X, rasterTile.Id.Y, 17);
        if(_map != null)
            tileMesh.transform.localPosition = _map.GeoToWorldPosition(centerLatLong, false);
    }

    private void PrintLog(string message)
    {
        if (!DoesLog) return;
        Log += message;
    }
    #endregion

    public int GetTileCount(int _zoomLevel, string _topLeft, string _bottomRight)
    {
        ZoomLevel = _zoomLevel;
        Points = new List<string>();
        Points.Add(_topLeft);
        Points.Add(_bottomRight);

        var pointMeters = new List<UnwrappedTileId>();
        foreach (var point in Points)
        {
            var pointVector = Conversions.StringToLatLon(point);
            var pointMeter = Conversions.LatitudeLongitudeToTileId(pointVector.x, pointVector.y, ZoomLevel);
            pointMeters.Add(pointMeter);
        }

        var minx = int.MaxValue;
        var maxx = int.MinValue;
        var miny = int.MaxValue;
        var maxy = int.MinValue;

        foreach (var meter in pointMeters)
        {
            if (meter.X < minx)
            {
                minx = meter.X;
            }

            if (meter.X > maxx)
            {
                maxx = meter.X;
            }

            if (meter.Y < miny)
            {
                miny = meter.Y;
            }

            if (meter.Y > maxy)
            {
                maxy = meter.Y;
            }
        }

        if (maxx == minx)
        {
            maxx++;
            minx--;
        }

        if (maxy == miny)
        {
            maxy++;
            miny--;
        }

        return (maxx - minx) * (maxy - miny);
    }

    public void ClearCache()
    {
        MapboxAccess.Instance.ClearAllCacheFiles();
        EntryManager.downloadedTiles = 0;
        PlayerPrefs.SetInt(EntryManager.keyDownloadedTiles, 0);
        UpdateTileLimitText(0);
    }

    public void UpdateTileLimitText(int value)
    {
        textTilesLimit.text = value.ToString() + "/3000";
        PlayerPrefs.SetInt(EntryManager.keyDownloadedTiles, value);
    }

    public void ResetProgressBar()
    {
        if (progressBarImage != null && progressBarImage.gameObject.activeInHierarchy)
        {
            progressBarImage.fillAmount = 0f;
            progressBarText.text = "Progress";
        }
    }
}
