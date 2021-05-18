using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
using System.Collections.Generic;

// vectors have only 1 value for now, 0 index only

public enum ZoomAction{
	ZoomIn,
	ZoomOut
}

public class SpawnOnMapCustom : MonoBehaviour
{
	public static SpawnOnMapCustom instance;

	[SerializeField]
	AbstractMap _map;

	[SerializeField]
	[Geocode]
	string[] _locationStrings;
	Vector2d[] _locations;

	[SerializeField]
	float _spawnScale = 1f;

	[SerializeField]
	GameObject[] _markerPrefabs;

	List<GameObject> _spawnedObjects;

	[Header("Map Camera Parameters")]
	[SerializeField] Camera _mapCamera;

	[Header("Marker Parameters")]
	[SerializeField] float _locationSetPeriod = 0.5f;		// this should match with data retrive period
	[SerializeField] float _yPosition = 10f;
	[SerializeField] float _markerScale = 1f;
	float _baseScaleFactor = 50f;
	Vector2d[] _diffValues;

	const float WIDTH = 51.32f;
	const float HEIGHT = 45.92f;
	float prevDragX = 0f;
	float prevDragZ = 0f;
	float currentDragX = 0f;
	float currentDragZ = 0f;

	[Header("Scroll Wheel Sensitivity")]
	[SerializeField] float sensitivityValue = 10f;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
	}

	void Start()
	{
		// set proper scale for markers on map, 
		float currentScale = _mapCamera.transform.position.y / _baseScaleFactor;
		_spawnScale = currentScale * _markerScale;

		_locations = new Vector2d[_locationStrings.Length];
		_diffValues = new Vector2d[_locationStrings.Length];

		_spawnedObjects = new List<GameObject>();
		for (int i = 0; i < _locationStrings.Length; i++)
		{
			var locationString = _locationStrings[i];
			_locations[i] = Conversions.StringToLatLon(locationString);
			var instance = Instantiate(_markerPrefabs[i]);

			Vector3 rawPosition = _map.GeoToWorldPosition(_locations[i], true);
			instance.transform.localPosition = new Vector3(rawPosition.x, _yPosition, rawPosition.z);
			instance.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
			_spawnedObjects.Add(instance);
		}
	}

	private void Update()
	{
		// get scroll wheel data and assign to map camera y
		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if(scrollWheel < 0f && DraggingMap.isMouseInRegion)
        {
			if (_mapCamera.transform.position.y < 400f)
				_mapCamera.transform.position += new Vector3(0f, sensitivityValue, 0f);
		}
		if(scrollWheel > 0f && DraggingMap.isMouseInRegion)
        {
			if (_mapCamera.transform.position.y > 50f)
				_mapCamera.transform.position -= new Vector3(0f, sensitivityValue, 0f);

		}

		// current camera scale
		float currentScale = _mapCamera.transform.position.y / _baseScaleFactor;
		_spawnScale = currentScale * _markerScale;


		// MOVE CAMERA WITH MOUSE DRAG ###############################################################
		if (DraggingMap.isDragging)
        {
			prevDragX = currentDragX;
			currentDragX = DraggingMap.dragX;
			float diff1 = prevDragX - currentDragX;
			if(prevDragX != 0f && currentDragX != 0f)
				_mapCamera.transform.position += new Vector3(diff1 * currentScale * WIDTH, 0f, 0f);

			prevDragZ = currentDragZ;
			currentDragZ = DraggingMap.dragZ;
			float diff2 = prevDragZ - currentDragZ;
			if(prevDragZ != 0f && currentDragZ != 0f)
				_mapCamera.transform.position += new Vector3(0f, 0f, diff2 * currentScale * HEIGHT);
		}
		else
        {
			prevDragX = 0f;
			currentDragX = 0f;
			prevDragZ = 0f;
			currentDragZ = 0f;
        }
		// ###################################################################################################

		// location adding doesn't check if it reaches the target point, check it out
		// BASE POSITION SMOOTH UPDATE
		_locations[0] += (_diffValues[0] * Time.deltaTime / _locationSetPeriod);

		// ROCKET POSITION SMOOTH UPDATE
		_locations[1] += (_diffValues[1] * Time.deltaTime / _locationSetPeriod);


		//	SET MARKER LOCATIONS #################################################################################
		int count = _spawnedObjects.Count;
		for (int i = 0; i < count; i++)
		{
			var spawnedObject = _spawnedObjects[i];
			var location = _locations[i];

			Vector3 rawPosition = _map.GeoToWorldPosition(_locations[i], true);
			spawnedObject.transform.localPosition = new Vector3(rawPosition.x, _yPosition, rawPosition.z);
			//instance.transform.localPosition = _map.GeoToWorldPosition(_locations[i], true);

			spawnedObject.transform.localScale = new Vector3(_spawnScale, _spawnScale, _spawnScale);
		}
		//	#########################################################################################################
	}

	// assign random lat long periodically
	public void SetBasePosition(string locString)
    {
		// assign location difference
		Vector2d nextBasePosition = Conversions.StringToLatLon(locString);
		_diffValues[0] = nextBasePosition - _locations[0];
	}

	public void SetRocketPosition(string locString)
    {
		// assign location difference
		Vector2d nextRocketPosition = Conversions.StringToLatLon(locString);
		_diffValues[1] = nextRocketPosition - _locations[1];
	}

	// create AvionicBay location set method later
	// ...
}





/*

		// CAMERA ZOOM ##############################################################################
		// get zoom level
		double distanceLat = Mathd.Abs(_locations[1].x - _locations[0].x);
		double distanceLong = Mathd.Abs(_locations[1].y - _locations[0].y);
		int divideLat = (int) (distanceLat / _verticalBound) + 1;
		int divideLong = (int) (distanceLong / _horizontalBound) + 1;
		_zoomTargetIndex = divideLat;
		if (divideLong > divideLat)
			_zoomTargetIndex = divideLong;
		// get camera Y target and current values
		float camYTarget = _zoomTargetIndex * _baseScaleFactor;
		float currentYValue = _mapCamera.transform.position.y;
		
		if(Mathf.Abs(camYTarget-currentYValue)>= _verticalBound && _setCamYDiff == false)
        {
			_setCamYDiff = true;
			_camYDiff = camYTarget - currentYValue;
		}
		// set new y value for map camera
		if( ((camYTarget - currentYValue) * _camYDiff) <= 0f)
        {
			// set target to current value to make it precise
			//Vector3 valueToSet = _mapCamera.transform.position;
			//valueToSet.y = camYTarget;
			//_mapCamera.transform.position = valueToSet;
			_camYDiff = 0f;
			_setCamYDiff = false;
		}
        else
			_mapCamera.transform.position += new Vector3(0f, _camYDiff * Time.deltaTime / _zoomSetTime, 0f);
		// ################################################################################################### 
 
*/


/*		BASE POSITION SET CODE 
 *		
			// CAMERA TO BASE LOCATION
		Vector3 tempPos = _map.GeoToWorldPosition(nextLocationBase, false);
		Vector3 nextCamPos = new Vector3(tempPos.x, _mapCamera.transform.position.y, tempPos.z);
		_cameraMoveDiff = nextCamPos - _mapCamera.transform.position;

		// ADD ZOOM ACTION DEPENDING ON THE CAMERA CENTER NOT BASE LOCATION
		double distanceLat = Mathd.Abs(_locations[1].x - _locations[0].x);
		double distanceLong = Mathd.Abs(_locations[1].y - _locations[0].y);
		int divideLat = (int)(distanceLat / _verticalBound) + 1;
		int divideLong = (int)(distanceLong / _horizontalBound) + 1;
		int zoomTargetIndex = divideLat;
		if (divideLong > divideLat)
			zoomTargetIndex = divideLong;
		int zoomDiff = zoomTargetIndex - _zoomLevel;
		for(int i=0; i<(Mathd.Abs(zoomDiff)); i++)
        {
			if (zoomDiff > 0)
				_zoomActions.Add(ZoomAction.ZoomOut);
			else if (zoomDiff < 0)
				_zoomActions.Add(ZoomAction.ZoomIn);	
        }
		_zoomLevel = zoomTargetIndex;


		CODE USED TO BE IN UPDATE

		// FOLLOW BASE MARKER
		_mapCamera.transform.position += (_cameraMoveDiff * Time.deltaTime / _cameraMovePeriod);


		// APPLY CAMERA ZOOM ACTIONS ######################################################################
		if(_zoomActions.Count > 0)
        {
			if(actionStarted == false)
            {
				actionStarted = true;
				initialYPos = _mapCamera.transform.position.y;
			}	
			
			ZoomAction currentAction = _zoomActions[0];
			int dir = 1;

			if (currentAction == ZoomAction.ZoomOut)
				dir = 1;
			else
				dir = -1;

			_mapCamera.transform.position += new Vector3(0f, dir * _baseScaleFactor * Time.deltaTime / _zoomSetTime, 0f);
			float targetValue = initialYPos + dir * _baseScaleFactor;
			float currentValue = _mapCamera.transform.position.y;
			if (((targetValue - currentValue) * dir) < 0f)
            {
				_mapCamera.transform.position = new Vector3(_mapCamera.transform.position.x, targetValue, 
						_mapCamera.transform.position.z);
				actionStarted = false;
				_zoomActions.RemoveAt(0);
			}
		}
		//	############################################################################################################
*/